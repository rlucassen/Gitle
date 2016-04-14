namespace Gitle.Web.Controllers
{
    using Model;
    using NHibernate;

    public class ReportPresetController : SecureController
    {
        public ReportPresetController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void New()
        {
            var reportPreset  = BindObject<ReportPreset>("item");
            reportPreset.User = CurrentUser;
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(reportPreset);
                transaction.Commit();
            }
            RedirectToReferrer();
        }

        public void Delete(long id)
        {
            var reportPreset = session.Get<ReportPreset>(id);
            reportPreset.Deactivate();
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(reportPreset);
                transaction.Commit();
            }
            RedirectToReferrer();
        }
    }
}