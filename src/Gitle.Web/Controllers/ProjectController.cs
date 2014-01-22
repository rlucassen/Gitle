namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Admin;
    using Castle.MonoRail.Framework;
    using Clients.Freckle.Interfaces;
    using Clients.GitHub.Interfaces;
    using Clients.GitHub.Models;
    using FluentNHibernate.Utils;
    using Model;
    using Model.Interfaces.Repository;
    using Helpers;
    using Label = Clients.GitHub.Models.Label;

    public class ProjectController : SecureController
    {
        private IProjectRepository repository;
        private IRepositoryClient client;
        private ILabelClient labelClient;
        private IProjectClient projectClient;
        private IIssueClient issueClient;
        private ILabelRepository labelRepository;

        public ProjectController(IRepositoryClient client, ILabelClient labelClient, IProjectRepository repository, IProjectClient projectClient, IIssueClient issueClient, ILabelRepository labelRepository)
        {
            this.repository = repository;
            this.client = client;
            this.labelClient = labelClient;
            this.projectClient = projectClient;
            this.issueClient = issueClient;
            this.labelRepository = labelRepository;
        }

        public void Index()
        {
            PropertyBag.Add("items", CurrentUser.IsAdmin ? repository.FindAll() : CurrentUser.Projects.Select(x => x.Project));
        }

        [MustHaveProject]
        public void View(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
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
            var issues = issueClient.List(project.Repository, project.MilestoneId);
            var doneTime = issues.Where(i => i.State == "closed").Sum(i => i.Hours);
            var totalIssueTime = issues.Sum(i => i.Hours);
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
            PropertyBag.Add("repositories", client.List());
            PropertyBag.Add("freckleProjects", projectClient.List());
            PropertyBag.Add("item", new Project());
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            PropertyBag.Add("repositories", client.List());
            PropertyBag.Add("freckleProjects", projectClient.List());
            PropertyBag.Add("item", project);
        }

        [Admin]
        public void Delete(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            project.Deactivate();
            repository.Save(project);
            RedirectToReferrer();
        }

        [Admin]
        public void Save(string projectSlug, [DataBind("label")] Model.Label[] labels)
        {
            var item = repository.FindBySlug(projectSlug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Project>("item");
            }

            item.Labels.Clear();
            labels.Each(l =>
                            {
                                l.Project = item;
                                item.Labels.Add(l);
                            });

            repository.Save(item);

            CreateInitialLabels(item);
            CreateHooks(item);

            RedirectToUrl("/projects");
        }

        private void CreateInitialLabels(Project project)
        {
            var labels = labelClient.List(project.Repository);

            foreach (var label in project.Labels)
            {
                var ghLabel = labels.FirstOrDefault(l => l.Name == label.Name);
                if (ghLabel != null)
                {
                    // if label exists check if color has changed
                    if (ghLabel.Color != label.Color)
                    {
                        labelClient.Patch(project.Repository, ghLabel.Name,
                                          new Label { Name = label.Name, Color = label.Color });
                    }
                }
                else
                {
                    labelClient.Post(project.Repository, new Label {Name = label.Name, Color = label.Color});
                }
            }
        }

        private void CreateHooks(Project project)
        {
            var hooks = client.GetHooks(project.Repository);

            var url = string.Format("{0}://{1}/githubhook/hook", Request.Uri.Scheme, Request.Uri.Authority);
            if (!hooks.Any(h => h.Events.Contains("issues") && h.Events.Contains("issue_comment") && h.Config["url"] == url))
            {
                var postHook = client.PostHook(project.Repository, url);
            }
        }


    }
}