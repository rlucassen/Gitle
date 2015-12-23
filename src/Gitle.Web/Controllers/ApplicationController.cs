namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using Model.Interfaces.Service;
    using Model.Nested;
    using NHibernate;
    using NHibernate.Linq;

    public class ApplicationController : SecureController
    {
        public ApplicationController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void Index()
        {
            PropertyBag.Add("items", session.Query<Application>().Where(x => x.IsActive).ToList());
        }

        public void New()
        {
            PropertyBag.Add("item", new Application());
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            RenderView("edit");
        }

        public void Edit(string applicationSlug)
        {
            var application = session.Query<Application>().FirstOrDefault(x => x.Slug == applicationSlug);
            PropertyBag.Add("item", application );
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            if (application != null) PropertyBag.Add("customerId", application.Customer.Id);
        }

        public void View(string applicationSlug)
        {
            var application = session.Query<Application>().FirstOrDefault(x => x.Slug == applicationSlug);
            PropertyBag.Add("item", application);
        }

        public void Save(string applicationSlug, long customerId)
        {
            var item = session.Query<Application>().FirstOrDefault(x => x.IsActive && x.Slug == applicationSlug);
            var customer = session.Query<Customer>().FirstOrDefault(x => x.IsActive && x.Id == customerId);
         

            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Application>("item");

            }

            item.Customer = customer;
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/application/index");
        }

        public void Delete(string applicationSlug)
        {
            var application = session.Query<Application>().FirstOrDefault(x => x.IsActive && x.Slug == applicationSlug);
            application.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(application);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        public void Comments(string applicationSlug, string comment)
        {
            var item = session.Query<Application>().FirstOrDefault(p => p.Slug == applicationSlug);

            item.Comments = comment;

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RenderText(comment);
        }
    }
}
