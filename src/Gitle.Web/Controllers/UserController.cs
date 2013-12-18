namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Admin;
    using FluentNHibernate.Utils;
    using Helpers;
    using Model;
    using Model.Interfaces.Repository;
    using Model.Nested;

    public class UserController : SecureController
    {
        private readonly IUserRepository userRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IUserProjectRepository userProjectRepository;

        public UserController(IUserRepository userRepository, IProjectRepository projectRepository, IUserProjectRepository userProjectRepository)
        {
            this.userRepository = userRepository;
            this.projectRepository = projectRepository;
            this.userProjectRepository = userProjectRepository;
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", userRepository.FindAll());
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new User());
            PropertyBag.Add("selectedprojects", new List<Project>());
            PropertyBag.Add("notificationprojects", new List<Project>());
            PropertyBag.Add("projects", projectRepository.FindAll());
            RenderView("edit");
        }

        [Admin]
        public void Edit(int userId)
        {
            var user = userRepository.Get(userId);
            PropertyBag.Add("item", user);
            PropertyBag.Add("selectedprojects", user.Projects.Select(up => up.Project).ToList());
            PropertyBag.Add("notificationprojects", user.Projects.Where(p => p.Notifications).Select(up => up.Project).ToList());
            PropertyBag.Add("projects", projectRepository.FindAll());
        }

        [Admin]
        public void Delete(int userId)
        {
            var user = userRepository.Get(userId);
            user.Deactivate();
            userRepository.Save(user);
            RedirectToReferrer();
        }

        [Admin]
        public void Save(int userId, string password, int[] selectedprojects, int[] notificationprojects)
        {
            User item = userRepository.Get(userId);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<User>("item");
            }
            if (!string.IsNullOrEmpty(password))
            {
                item.Password = new Password(password);
            }item.Projects.Clear();
            
            selectedprojects.Select(p => new UserProject {Project = projectRepository.Get(p), User = item, Notifications = notificationprojects.ToList().Contains(p)}).Each(item.Projects.Add);
            userRepository.Save(item);

            RedirectToUrl("/users");
        }
    }
}