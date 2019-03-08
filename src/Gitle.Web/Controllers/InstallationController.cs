namespace Gitle.Web.Controllers
{
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
            PropertyBag.Add("items", session.Query<Installation>().Where(x => x.IsActive).ToList());
        }

        [Admin]
        public void View(string installationSlug)
        {
            Installation installation = session.SlugOrDefault<Installation>(installationSlug);

            PropertyBag.Add("item", installation);
        }

        [Admin]
        public void Edit()
        {
            var installationTypes = EnumHelper.ToDictionary(typeof(InstallationType));
            var types = installationTypes.Where(t =>
                    new[] { InstallationType.Live, InstallationType.Acceptance, InstallationType.Demo }.Contains((InstallationType)t.Key))
                .ToList();

            PropertyBag.Add("applications", session.Query<Application>().Where(x => x.IsActive).OrderBy(x => x.Name));
            PropertyBag.Add("servers", session.Query<Server>().Where(x => x.IsActive).OrderBy(x => x.Name));
            PropertyBag.Add("installationTypes", types);
            PropertyBag.Add("item", new Installation());
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
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Installation>("item");
            }

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