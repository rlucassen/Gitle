namespace Gitle.Web.Controllers
{
    #region Usings

    using System.Collections.Generic;
    using System.Web.Security;
    using Model.Interfaces.Repository;

    #endregion

    public class AuthenticationController : BaseController
    {
        private readonly IUserRepository userRepository;

        public AuthenticationController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public void Index(string name, string password, bool persistent)
        {
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(password)) return;

            FormsAuthentication.Initialize();
            
            var user = userRepository.FindByName(name);

            if (!user.Password.Match(password))
            {
                Error("Inloggen niet gelukt", true);
                return;
            }

            FormsAuthentication.SetAuthCookie(name, persistent);

            RedirectUsingRoute("admin","home","index", false);
        }

        public void Signout()
        {
            FormsAuthentication.SignOut();
            RedirectToSiteRoot();
        }
    }
}