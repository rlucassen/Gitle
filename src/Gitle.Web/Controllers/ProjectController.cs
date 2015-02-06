namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Clients.Freckle.Interfaces;
    using FluentNHibernate.Utils;
    using Model;
    using Helpers;
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
            var donePercentage = doneTime * 100.0 / totalIssueTime;

            PropertyBag.Add("doneTime", doneTime);
            PropertyBag.Add("donePercentage", donePercentage);
            PropertyBag.Add("totalIssueTime", totalIssueTime);

            PropertyBag.Add("customers", project.Users.Where(up => !up.User.IsAdmin));
            PropertyBag.Add("developers", project.Users.Where(up => up.User.IsAdmin));
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("freckleProjects", projectClient.List().Where(x => x.Enabled));
            PropertyBag.Add("item", new Project());
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Slug == projectSlug);
            PropertyBag.Add("freckleProjects", projectClient.List().Where(x => x.Enabled));
            PropertyBag.Add("item", project);
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
        public void Save(string projectSlug)
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

            var labels = BindObject<Label[]>("label");

            var labelsToDelete = item.Labels.Where(l => !labels.Select(x => x.Id).Contains(l.Id)).ToList();

            using (var tx = session.BeginTransaction())
            {
                foreach (var label in labels.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                {
                    session.Merge(label);
                }
                foreach (var label in labelsToDelete)
                {
                    item.Labels.Remove(label);
                    session.Delete(label);
                }
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/projects");
        }

        [MustHaveProject]
        public void AddLabel(string projectSlug, string issues, string label)
        {
            var issueIds = issues.Split(',');
            var realLabel = session.Query<Label>().FirstOrDefault(x => x.Name == label);
            if (!realLabel.ApplicableByCustomer && !CurrentUser.IsAdmin)
            {
                RedirectToReferrer();
                return;
            }

            foreach (var issueId in issueIds.Select(id => int.Parse(id)))
            {
                var issue = session.Query<Issue>().FirstOrDefault(x => x.Number == issueId);
                issue.Labels.Add(realLabel);
            }

            RedirectToReferrer();
        }
    }
}