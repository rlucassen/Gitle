namespace Gitle.Web.Controllers
{
    using Model;
    using NHibernate;

    public class FilterPresetController : SecureController
    {
        public FilterPresetController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void New(long projectId)
        {
            var filterPreset = BindObject<FilterPreset>("item");
            filterPreset.User = CurrentUser;
            filterPreset.Project = session.Get<Project>(projectId);
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(filterPreset);
                transaction.Commit();
            }
            RedirectToReferrer();
        }
    }
}