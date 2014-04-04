namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Web;
    using Admin;
    using Clients.Freckle.Interfaces;
    using Clients.Freckle.Models;
    using Clients.GitHub.Interfaces;
    using Clients.GitHub.Models;
    using Model;
    using Model.Helpers;
    using Model.Interfaces.Repository;
    using Helpers;
    using Newtonsoft.Json;
    using Label = Clients.GitHub.Models.Label;
    using Project = Model.Project;

    public class IssueController : SecureController
    {
        private readonly IIssueClient client;
        private readonly ICommentClient commentClient;
        private readonly IProjectRepository repository;
        private readonly IMilestoneClient milestoneClient;
        private readonly ILabelClient labelClient;
        private readonly IEntryClient entryClient;

        public IssueController(IProjectRepository repository, IIssueClient client, ICommentClient commentClient, IMilestoneClient milestoneClient, ILabelClient labelClient, IEntryClient entryClient)
        {
            this.repository = repository;
            this.client = client;
            this.commentClient = commentClient;
            this.milestoneClient = milestoneClient;
            this.labelClient = labelClient;
            this.entryClient = entryClient;
        }

        [MustHaveProject]
        public void Index(string projectSlug, string label, string state)
        {
            var project = repository.FindBySlug(projectSlug);
            state = string.IsNullOrEmpty(state) ? CurrentUser.DefaultState : state;
            var items = client.List(project.Repository, project.MilestoneId, state);

            if (!string.IsNullOrEmpty(label) && label != "0")
                items = items.Where(i => i.Labels.Any(l => l.Name == label)).ToList();

            PropertyBag.Add("items", items);
            PropertyBag.Add("project", project);
            PropertyBag.Add("label", label);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.VisibleForCustomer));
            PropertyBag.Add("customerLabels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
            PropertyBag.Add("state", state);
        }

        [MustHaveProject]
        public void View(string projectSlug, int issueId)
        {
            var project = repository.FindBySlug(projectSlug);
            PropertyBag.Add("project", project);
            var item = client.Get(project.Repository, issueId);
            PropertyBag.Add("item", item);
            var comments = commentClient.List(project.Repository, issueId);
            PropertyBag.Add("comments", comments);
            PropertyBag.Add("days", DayHelper.GetPastDaysList());
        }

        [MustHaveProject]
        public void QuickView(string projectSlug, int issueId)
        {
            var project = repository.FindBySlug(projectSlug);
            PropertyBag.Add("project", project);
            var item = client.Get(project.Repository, issueId);
            PropertyBag.Add("item", item);
            var comments = commentClient.List(project.Repository, issueId);
            PropertyBag.Add("comments", comments);
            CancelLayout();
        }

        [MustHaveProject]
        public void New(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            PropertyBag.Add("project", project);
            PropertyBag.Add("item", new Issue());
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug, int issueId)
        {
            var project = repository.FindBySlug(projectSlug);
            PropertyBag.Add("project", project);
            var item = client.Get(project.Repository, issueId);
            PropertyBag.Add("item", item);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
        }

        [MustHaveProject]
        public void Save(string projectSlug, int issueId, string[] labels)
        {
            var project = repository.FindBySlug(projectSlug);
            var issue = client.Get(project.Repository, issueId);

            if (issue != null)
            {
                BindObjectInstance(issue, "item");
            }
            else
            {
                issue = BindObject<Issue>("item");
            }

            issue.Labels = labels.Select(label => labelClient.Get(project.Repository, label)).ToList();

            issue.Milestone = milestoneClient.Get(project.Repository, project.MilestoneId);

            if (issue.Number > 0)
            {
                client.Patch(project.Repository, issue.Number, issue);
            }
            else
            {
                client.Post(project.Repository, issue);
            }
            RedirectToUrl(string.Format("/project/{0}/issue/index", project.Slug));
        }

        [MustHaveProject]
        public void AddComment(string projectSlug, int issueId, string body)
        {
            var project = repository.FindBySlug(projectSlug);
            var comment = new Comment
                              {
                                  Body = string.Format("({0}): {1}", CurrentUser.FullName, body)
                              };
            commentClient.Post(project.Repository, issueId, comment);
            RedirectToReferrer();
        }

        [MustHaveProject]
        public void AddLabel(string projectSlug, int issueId, int param)
        {
            var project = repository.FindBySlug(projectSlug);
            var issue = client.Get(project.Repository, issueId);
            var label = project.Labels.First(l => l.Id == param);
            issue.Labels.Add(labelClient.Get(project.Repository, label.Name));
            client.Patch(project.Repository, issue.Number, issue);

            RedirectToReferrer();
        }

        [Admin]
        public void BookTime(string projectSlug, int issueId, string date, double hours, bool close)
        {
            if (hours <= 0.0)
            {
                Flash.Add("error", "Geen uren ingevuld, niets geboekt naar Freckle");
                RedirectToReferrer();
                return;
            }
            var project = repository.FindBySlug(projectSlug);
            var issue = client.Get(project.Repository, issueId);
            var labels = project.Labels.Where(l => l.ToFreckle && issue.Labels.Select(label => label.Name).Contains(l.Name)).Select(l => l.Name).ToList();
            var description = string.Format("{0}, !!#{1} - {2}", string.Join(",", labels), issue.Number, issue.Title);
            var entry = new Entry()
                            {
                                Date = date,
                                Description = description,
                                Minutes = string.Format("{0}h", hours),
                                ProjectId = project.FreckleId,
                                User = CurrentUser.FreckleEmail
                            };
            if (entryClient.Post(entry))
            {
                Flash.Add("info", "Uren geboekt in Freckle");
                if (close)
                {
                    IssueState(project, issue, "closed");
                }
            }
            else
                Flash.Add("error", "Er is iets mis gegaan met boeken in Freckle");
            
            RedirectToReferrer();
        }

        [Admin]
        public void IssueState(string projectSlug, int issueId, string state)
        {
            var project = repository.FindBySlug(projectSlug);
            var issue = client.Get(project.Repository, issueId);
            IssueState(project, issue, state);
            RedirectToReferrer();
        }

        private void IssueState(Project project, Issue issue, string state)
        {
            issue.State = state;
            client.Patch(project.Repository, issue.Number, issue);
        }

        [Admin]
        public void ExportImport(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);

            PropertyBag.Add("project", project);
        }

        [Admin]
        public void ExportJson(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            var issues = client.List(project.Repository, project.MilestoneId);

            var json = JsonConvert.SerializeObject(issues);

            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = string.Format("issues_{0}_{1:yyyyMMdd_hhmm}.json", project.Name, DateTime.Now);

            Response.AppendHeader("content-disposition", string.Format("attachment; filename={0}", filename));
            Response.ContentType = "application/json";

            var byteArray = Encoding.Default.GetBytes(json);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }

        [Admin]
        public void ReadJson(string projectSlug, HttpPostedFile import)
        {
            var project = repository.FindBySlug(projectSlug);
            var milestone = milestoneClient.Get(project.Repository, project.MilestoneId);

            var json = new StreamReader(import.InputStream).ReadToEnd();
            var issues = JsonConvert.DeserializeObject<List<Issue>>(json);

            var seskey = string.Format("{0:yyyyMMdd_hhmmss}", DateTime.Now);
            Session.Add(seskey, issues);

            PropertyBag.Add("seskey", seskey);
            PropertyBag.Add("milestone", milestone);
            PropertyBag.Add("items", issues);
            PropertyBag.Add("project", project);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.VisibleForCustomer));
        }


        [Admin]
        public void ImportJson(string projectSlug, string seskey)
        {
            var project = repository.FindBySlug(projectSlug);
            var issues = (List<Issue>) Session[seskey];

            foreach (var issue in issues)
            {
                client.Post(project.Repository, issue);
            }

            RedirectUsingNamedRoute("issues", new {projectSlug = project.Slug});
        }


        [Admin]
        public void ExportCsv(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            var issues = client.List(project.Repository, project.MilestoneId);

            var csv = CsvHelper.IssuesCsv(project, issues);
            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = string.Format("issues_{0}_{1:yyyyMMdd_hhmm}.csv", project.Name, DateTime.Now);

            Response.AppendHeader("content-disposition", string.Format("attachment; filename={0}", filename));
            Response.ContentType = "application/csv";

            var byteArray = Encoding.Default.GetBytes(csv);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }
    }
}