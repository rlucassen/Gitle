namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Descriptors;
    using Gitle.Model;
    using Gitle.Model.Helpers;
    using Gitle.Web.Helpers;
    using NHibernate;
    using NHibernate.Linq;

    public class ServerController : SecureController
    {
        public ServerController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", session.Query<Server>().Where(x => x.IsActive).ToList());
        }

        [Admin]
        public void View(string serverSlug)
        {
            var server = session.SlugOrDefault<Server>(serverSlug);
            PropertyBag.Add("item", server);
        }

        [Admin]
        public void Edit(string serverSlug)
        {
            var server = session.SlugOrDefault<Server>(serverSlug);
            List<Contact> contacts = session.Query<Contact>().Where(x => x.IsActive && !server.Contacts.Contains(x)).ToList();

            PropertyBag.Add("item", (server != null) ? server : new Server());
            PropertyBag.Add("contacts", contacts);
            RenderView("edit");
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new Server());
            RenderView("edit");
        }

        [Admin]
        public void Save(string serverSlug)
        {
            var item = session.SlugOrDefault<Server>(serverSlug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Server>("item");
            }

            var contacts = BindObject<Contact[]>("contact");

            using (var tx = session.BeginTransaction())
            {
                item.Contacts = new List<Contact>();

                foreach (var contact in contacts.Where(x => !string.IsNullOrWhiteSpace(x.FullName)))
                {
                    session.SaveOrUpdate(contact);
                    item.Contacts.Add(contact);
                }

                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/server");
        }

        [Admin]
        public void Delete(string serverSlug)
        {
            var server = session.SlugOrDefault<Server>(serverSlug);
            server.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(server);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [return: JSONReturnBinder]
        public object AddContact(string serverSlug, long addContactId)
        {
            var server = session.Slug<Server>(serverSlug);
            var contact = session.Get<Contact>(addContactId);

            return new
            {
                success = true,
                message = contact.FullName+" is toegevoegd",
                contact
            };
        }
    }
}