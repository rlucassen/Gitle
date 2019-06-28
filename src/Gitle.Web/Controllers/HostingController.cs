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

    public class HostingController : SecureController
    {
        public HostingController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", session.Query<Hosting>().Where(x => x.IsActive).ToList());
        }

        [Admin]
        public void View(string slug)
        {
            var server = session.SlugOrDefault<Hosting>(slug);
            PropertyBag.Add("item", server);
        }

        [Admin]
        public void Edit(string slug)
        {
            var item = session.SlugOrDefault<Hosting>(slug); 
            PropertyBag.Add("item", (item != null) ? item : new Hosting()); 
            RenderView("edit");
        } 

        [Admin]
        public void Save(string serverSlug)
        {
            var item = session.SlugOrDefault<Hosting>(serverSlug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Hosting>("item");
            }

            var contacts = BindObject<Contact[]>("contacts");

            using (var tx = session.BeginTransaction())
            { 
                session.SaveOrUpdate(item);

                foreach (var contact in item.Contacts)
                {
                    session.Delete(contact); 
                }
 
                item.Contacts = new List<Contact>();

                foreach (var contact in contacts)
                {
                    if (string.IsNullOrEmpty(contact.FullName) || string.IsNullOrEmpty(contact.Email))
                    {
                        continue;
                    }
                    session.SaveOrUpdate(contact);
                    contact.Hosting = item;
                    item.Contacts.Add(contact);
                }

                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/hosting");
        }

        [Admin]
        public void Delete(string slug)
        {
            var item = session.SlugOrDefault<Hosting>(slug);
            item.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }
            RedirectToReferrer();
        }
    }
}