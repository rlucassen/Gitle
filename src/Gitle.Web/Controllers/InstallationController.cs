namespace Gitle.Web.Controllers
{
    using System.Linq;
    using Gitle.Model;
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
        public void Edit(string installationSlug)
        {
            Installation installation = session.SlugOrDefault<Installation>(installationSlug);

            PropertyBag.Add("item", installation);
        }
        
        [Admin]
        public void Delete()
        {

        }
    }
}