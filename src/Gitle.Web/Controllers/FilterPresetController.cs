namespace Gitle.Web.Controllers
{
    using Model;
    using Model.Helpers;
    using NHibernate;

    public class FilterPresetController : SecureController
    {
        private readonly ISession session;

        public FilterPresetController(ISessionFactory sessionFactory)
        {
            session = sessionFactory.GetCurrentSession();
        }
         public void New()
         {
             var filterPreset = BindObject<FilterPreset>("item");
             filterPreset.User = CurrentUser;
             using (var transaction = session.BeginTransaction())
             {
                 session.SaveOrUpdate(filterPreset);
                 transaction.Commit();
             }
             RedirectToReferrer();
         }
    }
}