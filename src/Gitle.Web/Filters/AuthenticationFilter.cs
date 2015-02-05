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

    public class AuthenticationFilter : IFilter
    {
        private readonly ISession session;

        public AuthenticationFilter(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.GetCurrentSession();
        }

        #region Implementation of IFilter

        public bool Perform(ExecuteWhen exec, IEngineContext context, IController controller, IControllerContext controllerContext)
        {
            var user = session.Query<User>().FirstOrDefault(x => x.Name == context.CurrentUser.Identity.Name);
            return user.IsActive;
        }

        #endregion
    }
}