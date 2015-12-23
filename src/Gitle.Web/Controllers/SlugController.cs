namespace Gitle.Web.Controllers
{
    using Model.Helpers;
    using NHibernate;

    public class SlugController : BaseController
    {
        public SlugController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void Index(string text)
         {
             CancelLayout();
             RenderText(string.IsNullOrEmpty(text) ? string.Empty : text.Slugify());
         }
    }
}