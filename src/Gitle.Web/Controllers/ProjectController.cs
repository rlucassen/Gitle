namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Clients.Freckle.Interfaces;
    using FluentNHibernate.Utils;
    using Model;
    using Helpers;
    using Model.Enum;
    using NHibernate;
    using NHibernate.Linq;

    public class ProjectController : SecureController
    {
        private ISession session;
        private IProjectClient projectClient;

        public ProjectController(ISessionFactory sessionFactory, IProjectClient projectClient)
        {
            this.session = sessionFactory.GetCurrentSession();
            this.projectClient = projectClient;
        }

        public void Index()
        {
            if (CurrentUser.Projects.Count == 1 && !CurrentUser.IsAdmin) RedirectUsingNamedRoute("issues", new {projectSlug = CurrentUser.Projects.First().Project.Slug});
            PropertyBag.Add("items", CurrentUser.IsAdmin ? session.Query<Project>().Where(x => x.IsActive).ToList() : CurrentUser.Projects.Select(x => x.Project).Where(x => x.IsActive));
        }

        [MustHaveProject]
        public void View(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Slug == projectSlug);
            PropertyBag.Add("project", project);

            var application = session.Query<Application>().FirstOrDefault(x => x.Projects.Contains(project));

            if (project.FreckleId > 0 && CurrentUser.IsAdmin)
            {
                var freckleProject = projectClient.Show(project.FreckleId);

                var bookedTime = freckleProject.BillableMinutes/60.0;
                var totalTime = freckleProject.BudgetMinutes/60.0;

                var bookedPercentage = bookedTime*100.0/totalTime;

                PropertyBag.Add("bookedTime", bookedTime);
                PropertyBag.Add("bookedPercentage", bookedPercentage);
                PropertyBag.Add("totalTime", totalTime);
            }
            var issues = session.Query<Issue>().Where(x => x.Project == project).ToList();
            var doneTime = issues.Where(i => !i.IsOpen).Sum(i => i.TotalHours);
            var totalIssueTime = issues.Sum(i => i.TotalHours);
            var donePercentage = doneTime*100.0/totalIssueTime;
            
            PropertyBag.Add("doneTime", doneTime);
            PropertyBag.Add("donePercentage", donePercentage);
            PropertyBag.Add("totalIssueTime", totalIssueTime);
            PropertyBag.Add("application", application);
            PropertyBag.Add("customers", project.Users.Where(up => !up.User.IsAdmin));
            PropertyBag.Add("developers", project.Users.Where(up => up.User.IsAdmin));
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("freckleProjects", projectClient.List().Where(x => x.Enabled));
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            PropertyBag.Add("applications", session.Query<Application>().Where(x => x.IsActive));
            PropertyBag.Add("item", new Project());
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Slug == projectSlug);
            PropertyBag.Add("freckleProjects", projectClient.List().Where(x => x.Enabled));
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            PropertyBag.Add("applications", session.Query<Application>().Where(x => x.IsActive));
            PropertyBag.Add("applicationId", session.Query<Application>().Where(x => x.Projects.Contains(project)));
            PropertyBag.Add("item", project);
            PropertyBag.Add("customerId", project.Customer?.Id);
        }

        [Admin]
        public void Delete(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Slug == projectSlug);
            project.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(project);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [Admin]
        public void Save(string projectSlug, long applicationId)
        {
            var item = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Slug == projectSlug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Project>("item");
            }

            var application = session.Get<Application>(applicationId);
            application.Projects.Add(item);
            session.SaveOrUpdate(application);
            var labels = BindObject<Label[]>("label");

            var labelsToDelete = item.Labels.Where(l => !labels.Select(x => x.Id).Contains(l.Id)).ToList();

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                foreach (var label in labels.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                {
                    label.Project = item;
                    session.Merge(label);
                }
                foreach (var label in labelsToDelete)
                {
                    item.Labels.Remove(label);
                    session.Delete(label);
                }
                tx.Commit();
            }

            RedirectToUrl("/projects");
        }

        [MustHaveProject]
        public void AddLabel(string projectSlug, string issues, string label)
        {
            var project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Slug == projectSlug);
            var issueIds = issues.Split(',');
            var realLabel = session.Query<Label>().FirstOrDefault(x => x.Name == label);
            if (!realLabel.ApplicableByCustomer && !CurrentUser.IsAdmin)
            {
                RedirectToReferrer();
                return;
            }

            using (var transaction = session.BeginTransaction())
            {
                foreach (var issueId in issueIds.Select(int.Parse))
                {
                    var issue = session.Query<Issue>().FirstOrDefault(x => x.Number == issueId && x.Project == project);
                    if (!issue.Labels.Contains(realLabel)) issue.Labels.Add(realLabel);
                    session.SaveOrUpdate(issue);
                }

                transaction.Commit();
            }

            RedirectToReferrer();
        }

        [MustHaveProject]
        public void ChangeState(string projectSlug, string issues, IssueState state)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issueIds = issues.Split(',');

            using (var transaction = session.BeginTransaction())
            {
                foreach (var issueId in issueIds.Select(int.Parse))
                {
                    var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
                    issue.ChangeState(CurrentUser, state);
                    session.SaveOrUpdate(issue);
                }

                transaction.Commit();
            }

            RedirectToReferrer();
        }

        [Admin]
        public void Comments(string projectSlug, string comment)
        {
            var item = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

            item.Comments = comment;

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RenderText(comment);
        }

        public void DeleteDocument(string projectSlug, long id)
        {
            var item = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var document = session.Get<Document>(id);

            item.Documents.Remove(document);


            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [return: JSONReturnBinder]
        public object Autocomplete(string query)
        {
            var suggestions = new List<Suggestion>();
            var projects = session.Query<Project>().Where(p => p.Name.Contains(query));
            suggestions.AddRange(projects.Select(x => new Suggestion(x.Name, x.Id.ToString())));
            return new { query = query, suggestions = suggestions };
        }
    }
}