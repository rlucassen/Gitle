namespace Gitle.Web.Controllers
{
    using System.Linq;
    using Gitle.Model;
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
            //PropertyBag.Add("items", session.Query<Server>().Where(x => x.IsActive).ToList());
        }
    }
}