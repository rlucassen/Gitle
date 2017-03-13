namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using Castle.MonoRail.Framework;
    using FluentNHibernate.Utils;
    using Helpers;
    using Model;
    using Model.Helpers;
    using Model.Nested;
    using NHibernate;
    using NHibernate.Linq;

    public class UserController : SecureController
    {
        public UserController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", session.Query<User>().Where(user => user.IsActive).ToList());
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new User());
            PropertyBag.Add("selectedprojects", new List<Project>());
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            PropertyBag.Add("notificationprojects", new List<Project>());
            PropertyBag.Add("projects", session.Query<Project>().Where(x => x.IsActive).OrderBy(x => x.Name).ToList());
            RenderView("edit");
        }

        [Admin]
        public void Edit(long userId)
        {
            var user = session.Get<User>(userId);
            PropertyBag.Add("item", user);
            PropertyBag.Add("selectedprojects", user.Projects.Select(x => x.Project).ToList());
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            PropertyBag.Add("projects", session.Query<Project>().Where(x => x.IsActive).OrderBy(x => x.Name).ToList());
            PropertyBag.Add("customerId", user.Customer?.Id ?? 0);
        }

        [Admin]
        public void View(long userId)
        {
            var user = session.Get<User>(userId);
            PropertyBag.Add("item", user);
        }

        public void Profile()
        {
            PropertyBag.Add("item", CurrentUser);
            PropertyBag.Add("projects", CurrentUser.Projects.Select(up => up.Project).ToList());
            PropertyBag.Add("notificationprojects", CurrentUser.Projects.Where(up => up.Notifications).Select(up => up.Project).ToList());
            PropertyBag.Add("ownnotificationprojects", CurrentUser.Projects.Where(up => up.Notifications && up.OnlyOwnIssues).Select(up => up.Project).ToList());
        }

        public void SaveProfile(string password, int[] notificationprojects, int[] ownnotificationprojects, long[] filterpresets, long customerId)
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
            
            if (!string.IsNullOrWhiteSpace(password) || item.Password == null)
            {
                item.Password = new Password(password);
            }

            item.Customer = session.Get<Customer>(customerId);

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
        public void Delete(long userId)
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
        public void Save(long userId, string password, long customerId)
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

            if (!string.IsNullOrWhiteSpace(password) || item.Password == null)
            {
                item.Password = new Password(password);
            }

            item.Customer = session.Get<Customer>(customerId);

            var userProjects = BindObject<UserProject[]>("userProject");

            var subscriptions = userProjects.Where(x => x.Subscribed).ToList();

            var userProjectsToDelete = item.Projects.Where(project => subscriptions.All(subscription => subscription.Id != project.Id)).ToList();

            foreach (var subscription in subscriptions)
            {
                var userProject = item.Projects.FirstOrDefault(x => x.Id == subscription.Id);
                if (userProject != null)
                {
                    userProject.Notifications = subscription.Notifications;
                    userProject.OnlyOwnIssues = subscription.OnlyOwnIssues;
                }
                else
                {
                    item.Projects.Add(subscription);
                }
            }

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);

                foreach (var userProject in userProjectsToDelete)
                {
                    userProject.User = null;
                    item.Projects.Remove(userProject);
                }
                tx.Commit();
            }

            RedirectToUrl("/users");
        }

        [Admin]
        public void Comments(long userId, string comment)
        {
            var item = session.Get<User>(userId);

            item.Comments = comment;

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RenderText(comment);
        }

        [return: JSONReturnBinder]
        public object CheckUserName(string name, long userId)
        {
            var validName = !session.Query<User>().Any(x => x.IsActive && x.Name == name && x.Id != userId);
            var message = "Voer een naam in";
            if (!validName)
            {
                message = "Deze naam is al in gebruik, kies een andere";
            }
            return new { success = validName, message = message };
        }


    }
}