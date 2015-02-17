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
        public void Edit(long userId)
        {
            var user = session.Get<User>(userId);
            PropertyBag.Add("item", user);
            PropertyBag.Add("selectedprojects", user.Projects.Select(x => x.Project).ToList());
            PropertyBag.Add("projects", session.Query<Project>().ToList());
        }

        public void Profile()
        {
            PropertyBag.Add("item", CurrentUser);
            PropertyBag.Add("projects", CurrentUser.Projects.Select(up => up.Project).ToList());
            PropertyBag.Add("notificationprojects", CurrentUser.Projects.Where(up => up.Notifications).Select(up => up.Project).ToList());
            PropertyBag.Add("ownnotificationprojects", CurrentUser.Projects.Where(up => up.Notifications && up.OnlyOwnIssues).Select(up => up.Project).ToList());
        }

        public void SaveProfile(string password, int[] notificationprojects, int[] ownnotificationprojects, long[] filterpresets)
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

            item.Projects.Each(up => up.OnlyOwnIssues = false);
            ownnotificationprojects.Each(i => item.Projects.First(up => up.Project.Id == i).OnlyOwnIssues = true);

            var filterPresetsToDelete = item.FilterPresets.Where(l => !filterpresets.Contains(l.Id)).ToList();

            using (var tx = session.BeginTransaction())
            {
                foreach (var filterPreset in filterPresetsToDelete)
                {
                    item.FilterPresets.Remove(filterPreset);
                    session.Delete(filterPreset);
                }
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
        public void Save(long userId, string password)
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

            var userProjects = BindObject<UserProject[]>("userProject");

            var userProjectsToDelete = item.Projects.Where(l => !userProjects.Where(x => x.Subscribed).Select(x => x.Id).Contains(l.Id)).ToList();

            using (var tx = session.BeginTransaction())
            {
                foreach (var userProject in userProjects.Where(x => x.Subscribed))
                {
                    session.Merge(userProject);
                }
                foreach (var userProject in userProjectsToDelete)
                {
                    item.Projects.Remove(userProject);
                    session.Delete(userProject);
                }
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/users");
        }
    }
}