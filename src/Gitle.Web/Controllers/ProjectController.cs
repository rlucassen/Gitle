namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Admin;
    using Clients.Freckle.Interfaces;
    using Clients.GitHub.Interfaces;
    using Clients.GitHub.Models;
    using Model;
    using Model.Interfaces.Repository;
    using Helpers;

    public class ProjectController : SecureController
    {
        private IProjectRepository repository;
        private ICustomerRepository customerRepository;
        private IRepositoryClient client;
        private ILabelClient labelClient;
        private IProjectClient projectClient;
        private IIssueClient issueClient;

        public ProjectController(IRepositoryClient client, ILabelClient labelClient, IProjectRepository repository, ICustomerRepository customerRepository, IProjectClient projectClient, IIssueClient issueClient)
        {
            this.repository = repository;
            this.client = client;
            this.labelClient = labelClient;
            this.customerRepository = customerRepository;
            this.projectClient = projectClient;
            this.issueClient = issueClient;
        }

        public void Index()
        {
            PropertyBag.Add("items", CurrentUser.IsAdmin ? repository.FindAll() : CurrentUser.Projects.Select(x => x.Project));
        }

        [Admin]
        public void View(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            var freckleProject = projectClient.Show(project.FreckleId);
            var issues = issueClient.List(project.Repository, project.MilestoneId);

            PropertyBag.Add("project", project);

            var doneTime = issues.Where(i => i.State == "closed").Sum(i => i.Hours);
            var bookedTime = freckleProject.BillableMinutes / 60.0;
            var totalTime = freckleProject.BudgetMinutes/60.0;

            var bookedPercentage = bookedTime*100.0/totalTime;
            var donePercentage = doneTime*100.0/totalTime;

            PropertyBag.Add("doneTime", doneTime);
            PropertyBag.Add("donePercentage", donePercentage);
            PropertyBag.Add("bookedTime", bookedTime);
            PropertyBag.Add("bookedPercentage", bookedPercentage);
            PropertyBag.Add("totalTime", totalTime);

            PropertyBag.Add("customers", project.Users.Where(up => !up.User.IsAdmin));
            PropertyBag.Add("developers", project.Users.Where(up => up.User.IsAdmin));
        }

        [Admin]
        public void Hooks(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            var hooks = client.GetHooks(project.Repository);
            if (hooks.Count == 0)
            {
                var postHook = client.PostHook(project.Repository, "http://ebf53b9.ngrok.com/githubhook/hook");
            }
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("repositories", client.List());
            PropertyBag.Add("customers", customerRepository.FindAll());
            PropertyBag.Add("item", new Project());
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug)
        {
            var project = repository.FindBySlug(projectSlug);
            PropertyBag.Add("repositories", client.List());
            PropertyBag.Add("customers", customerRepository.FindAll());
            PropertyBag.Add("customerId", project.Customer.Id);
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
        public void Save(string projectSlug, int customerId)
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
            var customer = customerRepository.Get(customerId);
            item.Customer = customer;
            repository.Save(item);

            var labels = labelClient.List(item.Repository);
            CreateInitialLabels(labels, item);

            RedirectToUrl("/projects");
        }

        private void CreateInitialLabels(List<Label> labels, Project project )
        {
            if (!labels.Select(l => l.Name).Contains("accepted"))
            {
                labelClient.Post(project.Repository, new Label() { Name = "accepted", Color = "008cba" });
            }
            if (!labels.Select(l => l.Name).Contains("invoiced"))
            {
                labelClient.Post(project.Repository, new Label() { Name = "invoiced", Color = "43ac6a" });
            }
            if (!labels.Select(l => l.Name).Contains("customer issue"))
            {
                labelClient.Post(project.Repository, new Label() { Name = "customer issue", Color = "f04124" });
            }
        }
    }
}