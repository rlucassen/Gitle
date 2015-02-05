namespace Gitle.Web.Filters
{
    #region Usings

    using System.Linq;
    using Castle.MonoRail.Framework;
    using Model;
    using NHibernate;
    using NHibernate.Linq;
    using IFilter = Castle.MonoRail.Framework.IFilter;

    #endregion

    public class AuthorisationFilter : IFilter
    {
        private readonly ISession session;

        public AuthorisationFilter(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.GetCurrentSession();
        }

        #region Implementation of IFilter

        public bool Perform(ExecuteWhen exec, IEngineContext context, IController controller, IControllerContext controllerContext)
        {
            if (!context.CurrentUser.Identity.IsAuthenticated)
            {
                //context.Response.Redirect("authentication", "index");
                context.Response.RedirectToUrl(string.Format("/signin?returnUrl={0}", context.Request.Url));
                return false;
            }

            var user = session.Query<User>().FirstOrDefault(x => x.Name == context.CurrentUser.Identity.Name);
            context.CurrentUser = user;

            controllerContext.PropertyBag["currentUser"] = user;

            return true;
        }

        #endregion
    }
}