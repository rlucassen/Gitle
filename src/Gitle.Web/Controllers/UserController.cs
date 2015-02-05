namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentNHibernate.Utils;
    using Helpers;
    using Model;
    using Model.Nested;
    using NHibernate;
    using NHibernate.Linq;

    public class UserController : SecureController
    {
        private readonly ISession session;

        public UserController(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.GetCurrentSession();
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", session.Query<User>().ToList());
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new User());
            PropertyBag.Add("selectedprojects", new List<Project>());
            PropertyBag.Add("notificationprojects", new List<Project>());
            PropertyBag.Add("projects", session.Query<Project>().ToList());
            RenderView("edit");
        }

        [Admin]
        public void Edit(int userId)
        {
            var user = session.Get<User>(userId);
            PropertyBag.Add("item", user);
            PropertyBag.Add("selectedprojects", user.Projects.Select(up => up.Project).ToList());
            PropertyBag.Add("notificationprojects", user.Projects.Where(p => p.Notifications).Select(up => up.Project).ToList());
            PropertyBag.Add("projects", session.Query<Project>().ToList());
        }

        public void Profile()
        {
            PropertyBag.Add("item", CurrentUser);
            PropertyBag.Add("projects", CurrentUser.Projects.Select(up => up.Project).ToList());
            PropertyBag.Add("notificationprojects", CurrentUser.Projects.Where(up => up.Notifications).Select(up => up.Project).ToList());
        }

        public void SaveProfile(string password, int[] notificationprojects)
        {
            var item = CurrentUser;
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
            }
            item.Projects.Each(up => up.Notifications = false);
            notificationprojects.Each(i => item.Projects.First(up => up.Project.Id == i).Notifications = true);
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }
            RedirectToSiteRoot();
        }

        [Admin]
        public void Delete(int userId)
        {
            var user = session.Get<User>(userId);
            user.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(user);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [Admin]
        public void Save(int userId, string password, int[] selectedprojects, int[] notificationprojects)
        {
            var item = session.Get<User>(userId);
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
            }
            
            item.Projects.Clear();
            selectedprojects.Select(p => new UserProject {Project = session.Get<Project>(p), User = item, Notifications = notificationprojects.ToList().Contains(p)}).Each(item.Projects.Add);

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }
            RedirectToUrl("/users");
        }
    }
}