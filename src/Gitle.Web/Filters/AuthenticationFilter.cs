namespace Gitle.Web.Filters
{
    #region Usings

    using Castle.MonoRail.Framework;
    using Model.Interfaces.Repository;

    #endregion

    public class AuthenticationFilter : IFilter
    {
        private readonly IUserRepository userRepository;

        public AuthenticationFilter(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        #region Implementation of IFilter

        public bool Perform(ExecuteWhen exec, IEngineContext context, IController controller, IControllerContext controllerContext)
        {
            var user = userRepository.FindByName(context.CurrentUser.Identity.Name);
            return user.IsActive;
        }

        #endregion
    }
}