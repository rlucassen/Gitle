namespace Gitle.Web.Controllers
{
    using System;
    using System.Linq;
    using Gitle.Model;
    using Gitle.Model.Enum;
    using Gitle.Web.Helpers;
    using Model.Helpers;
    using NHibernate;
    using NHibernate.Linq;

    public class InstallationController : SecureController
    {
        public InstallationController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index()
        {
            var items = session.Query<Installation>().Where(x => x.IsActive).ToList();

            PropertyBag.Add("items", items);
        }

        [Admin]
        public void View(string installationSlug)
        {
            Installation installation = session.SlugOrDefault<Installation>(installationSlug);

            PropertyBag.Add("item", installation);
        }

        [Admin]
        public void Edit(string installationSlug)
        {
            Installation installation = session.SlugOrDefault<Installation>(installationSlug);
            var installationTypes = EnumHelper.ToDictionary(typeof(InstallationType));
            var types = installationTypes.Where(t =>
                    new[] { InstallationType.Live, InstallationType.Acceptance, InstallationType.Demo }.Contains((InstallationType)t.Key))
                .ToList();

            PropertyBag.Add("applications", session.Query<Application>().Where(x => x.IsActive).OrderBy(x => x.Name));
            PropertyBag.Add("servers", session.Query<Server>().Where(x => x.IsActive).OrderBy(x => x.Name));
            PropertyBag.Add("installationTypes", types);
            PropertyBag.Add("item", (installation != null) ? installation : new Installation());
            RenderView("edit");
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new Installation());
            RenderView("edit");
        }

        [Admin]
        public void Save(string installationSlug)
        {
            var item = session.SlugOrDefault<Installation>(installationSlug);
            var appId = (Params["item.ApplicationId"] != null) ? long.Parse(Params["item.ApplicationId"]) : 0;
            var application = (appId != 0) ? session.Get<Application>(appId) : new Application();
            
            var server = session.Get<Server>(long.Parse(Params["item.ServerId"]));

            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Installation>("item");
            }

            item.Application = application;
            item.Server = server;

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/installation");
        }

        [Admin]
        public void Delete(string installationSlug)
        {
            var installation = session.SlugOrDefault<Installation>(installationSlug);
            installation.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(installation);
                tx.Commit();
            }
            RedirectToReferrer();
        }
    }
}