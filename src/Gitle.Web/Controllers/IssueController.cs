namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using Admin;
    using Clients.Freckle.Interfaces;
    using Clients.Freckle.Models;
    using Helpers;
    using Model;
    using Model.Helpers;
    using NHibernate;
    using NHibernate.Linq;
    using Newtonsoft.Json;
    using Issue = Model.Issue;
    using Project = Model.Project;

    public class IssueController : SecureController
    {
        private readonly ISession session;
        private readonly IEntryClient entryClient;

        public IssueController(ISessionFactory sessionFactory, IEntryClient entryClient)
        {
            this.session = sessionFactory.GetCurrentSession();
            this.entryClient = entryClient;
        }

        [MustHaveProject]
        public void Index(string projectSlug, string[] selectedLabels, string state)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            state = string.IsNullOrEmpty(state) ? CurrentUser.DefaultState : state;

            var itemsQuery =
                session.Query<Issue>().Where(x => x.Project == project).ToList()
                                      .Where(i => selectedLabels.All(sl => i.Labels.Select(l => l.Name).Contains(sl))).ToList();

            PropertyBag.Add("items", itemsQuery);
            PropertyBag.Add("project", project);
            PropertyBag.Add("selectedLabels", selectedLabels);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.VisibleForCustomer));
            PropertyBag.Add("customerLabels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
            PropertyBag.Add("state", state);
        }

        [MustHaveProject]
        public void View(string projectSlug, int issueId)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);
            PropertyBag.Add("item", item);
            PropertyBag.Add("comments", item.Comments);
            PropertyBag.Add("days", DayHelper.GetPastDaysList());
        }

        [MustHaveProject]
        public void QuickView(string projectSlug, int issueId)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);
            PropertyBag.Add("item", item);
            PropertyBag.Add("comments", item.Comments);
            CancelLayout();
        }

        [MustHaveProject]
        public void New(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            PropertyBag.Add("item", new Issue());
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug, int issueId)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);
            PropertyBag.Add("item", item);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
        }

        [MustHaveProject]
        public void Save(string projectSlug, int issueId, string[] labels)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);

            if (issue != null)
            {
                BindObjectInstance(issue, "item");
            }
            else
            {
                issue = BindObject<Issue>("item");
            }

            issue.Labels = labels.Select(label => session.Query<Label>().FirstOrDefault(l => l.Name == label)).ToList();

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(issue);
                transaction.Commit();
            }
            RedirectToUrl(string.Format("/project/{0}/issue/index", project.Slug));
        }

        [MustHaveProject]
        public void AddComment(string projectSlug, int issueId, string body)
        {
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);
            var comment = new Comment
                              {
                                  Text = body,
                                  CreatedAt = DateTime.Now,
                                  Issue = issue,
                                  User = CurrentUser
                              };
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(comment);
                transaction.Commit();
            }
            RedirectToReferrer();
        }

        [MustHaveProject]
        public void AddLabel(string projectSlug, int issueId, int param)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);
            var label = project.Labels.First(l => l.Id == param);
            issue.Labels.Add(label);

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(issue);
                transaction.Commit();
            }

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
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);
            var labels = project.Labels.Where(l => l.ToFreckle && issue.Labels.Select(label => label.Name).Contains(l.Name)).Select(l => l.Name).ToList();
            var description = string.Format("{0}, !!#{1} - {2}", string.Join(",", labels), issue.Number, issue.Name);
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
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId);
            IssueState(project, issue, state);
            RedirectToReferrer();
        }

        private void IssueState(Project project, Issue issue, string state)
        {
            issue.State = state;
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(issue);
                transaction.Commit();
            }
        }

        [Admin]
        public void ExportImport(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

            PropertyBag.Add("project", project);
        }

        [Admin]
        public void ExportJson(string projectSlug, string[] selectedLabels, string state, string issues)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

            state = string.IsNullOrEmpty(state) ? CurrentUser.DefaultState : state;
            var items = project.Issues;

            if (string.IsNullOrEmpty(issues))
            {
                if (selectedLabels.Length > 0)
                    items = items.Where(
                            i => i.Labels.Select(l => l.Name).Intersect(selectedLabels).Count() == selectedLabels.Length).ToList();
            }
            else
            {
                items = items.Where(i => issues.Split(',').Contains(i.Number.ToString())).ToList();
            }

            var json = JsonConvert.SerializeObject(items);

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

        //[Admin]
        //public void ReadJson(string projectSlug, HttpPostedFile import)
        //{
        //    var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

        //    var json = new StreamReader(import.InputStream).ReadToEnd();
        //    var issues = JsonConvert.DeserializeObject<List<Issue>>(json);

        //    var seskey = string.Format("{0:yyyyMMdd_hhmmss}", DateTime.Now);
        //    Session.Add(seskey, issues);

        //    PropertyBag.Add("seskey", seskey);
        //    PropertyBag.Add("milestone", milestone);
        //    PropertyBag.Add("items", issues);
        //    PropertyBag.Add("project", project);
        //    PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.VisibleForCustomer));
        //}


        //[Admin]
        //public void ImportJson(string projectSlug, string seskey)
        //{
        //    var project = repository.FindBySlug(projectSlug);
        //    var issues = (List<Issue>) Session[seskey];

        //    foreach (var issue in issues)
        //    {
        //        client.Post(project.Repository, issue);
        //    }

        //    RedirectUsingNamedRoute("issues", new {projectSlug = project.Slug});
        //}


        [Admin]
        public void ExportCsv(string projectSlug, string[] selectedLabels, string state, string issues)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

            state = string.IsNullOrEmpty(state) ? CurrentUser.DefaultState : state;
            var items = project.Issues;

            if (string.IsNullOrEmpty(issues))
            {
                if (selectedLabels.Length > 0)
                    items = items.Where(
                            i => i.Labels.Select(l => l.Name).Intersect(selectedLabels).Count() == selectedLabels.Length).ToList();
            }
            else
            {
                items = items.Where(i => issues.Split(',').Contains(i.Number.ToString())).ToList();
            }

            var csv = CsvHelper.IssuesCsv(project, items);
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