namespace Gitle.Web.Filters
{
    #region Usings

    using Castle.MonoRail.Framework;
    using Model;
    using Model.Interfaces.Repository;

    #endregion

    public class AuthorisationFilter : IFilter
    {
        private readonly IUserRepository userRepository;

        public AuthorisationFilter(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
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

            var user = userRepository.FindByName(context.CurrentUser.Identity.Name);
            context.CurrentUser = user;

            controllerContext.PropertyBag["currentUser"] = user;

            return true;
        }

        #endregion
    }
}