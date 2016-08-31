namespace Gitle.Web.Controllers
{
    using System.Linq;
    using Model;
    using Model.Enum;
    using NHibernate;
    using NHibernate.Linq;

    public class DashboardController : SecureController
    {
        public DashboardController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }
        public void Index()
        {
            var initialProjects = session.Query<Project>().Where(x => !x.Closed && x.Type == ProjectType.Initial);


        } 
    }
}