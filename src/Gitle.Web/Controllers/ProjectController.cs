namespace Gitle.Web.Controllers
{
    using Admin;
    using Clients.GitHub.Interfaces;
    using Model;
    using Model.Interfaces.Repository;
    using Helpers;

    public class ProjectController : SecureController
    {
        private IProjectRepository repository;
        private IRepositoryClient client;

        public ProjectController(IRepositoryClient client, IProjectRepository repository)
        {
            this.repository = repository;
            this.client = client;
        }

        public void Index()
        {
            PropertyBag.Add("items", CurrentUser.IsAdmin ? repository.FindAll() : CurrentUser.Projects);
        }

        public void New()
        {
            PropertyBag.Add("repositories", client.List());
            PropertyBag.Add("item", new Project());
            RenderView("edit");
        }

        [Admin]
        public void Edit(int projectId)
        {
            PropertyBag.Add("item", repository.Get(projectId));
            PropertyBag.Add("repositories", client.List());
        }

        [Admin]
        public void Save(int projectId)
        {
            var item = repository.Get(projectId);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Project>("item");
            }
            repository.Save(item);
            RedirectToUrl("/projects");
        }
    }
}