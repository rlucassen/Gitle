namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Clients.Freckle.Interfaces;
    using FluentNHibernate.Utils;
    using Model;
    using Helpers;
    using Model.Enum;
    using NHibernate;
    using NHibernate.Linq;

    public class CustomerController : SecureController
    {
        private ISession session;

        public CustomerController(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.GetCurrentSession();
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", session.Query<Customer>().Where(x => x.IsActive).ToList());
        }

        [Admin]
        public void View(string customerSlug)
        {
            var customer = session.Query<Customer>().FirstOrDefault(x => x.IsActive && x.Slug == customerSlug);
            PropertyBag.Add("customer", customer);

        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new Customer());
            RenderView("edit");
        }

        [Admin]
        public void Edit(string customerSlug)
        {
            var customer = session.Query<Customer>().FirstOrDefault(x => x.IsActive && x.Slug == customerSlug);
            PropertyBag.Add("item", customer);
        }

        [Admin]
        public void Delete(string customerSlug)
        {
            var customer = session.Query<Customer>().FirstOrDefault(x => x.IsActive && x.Slug == customerSlug);
            customer.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(customer);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [Admin]
        public void Save(string customerSlug)
        {
            var item = session.Query<Customer>().FirstOrDefault(x => x.IsActive && x.Slug == customerSlug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Customer>("item");
            }

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/customers");
        }
    }
}