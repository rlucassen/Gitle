namespace Gitle.Web.Controllers
{
    #region Usings

    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Web;
    using System.Web.Security;
    using Model.Helpers;
    using Model.Interfaces.Repository;
    using Model.Interfaces.Service;
    using Model.Nested;

    #endregion

    public class AuthenticationController : BaseController
    {
        private readonly IUserRepository userRepository;
        private readonly IEmailService emailService;

        public AuthenticationController(IUserRepository userRepository, IEmailService emailService)
        {
            this.userRepository = userRepository;
            this.emailService = emailService;
        }

        public void Index(string name, string password, bool persistent, string returnUrl)
        {
            PropertyBag.Add("githubClientId", ConfigurationManager.AppSettings["githubClientId"]);
            PropertyBag.Add("githubOAuthCallback", ConfigurationManager.AppSettings["githubOAuthCallback"]);
            PropertyBag.Add("githubScope", ConfigurationManager.AppSettings["githubScope"]);
            var state = HashHelper.GenerateHash();
            Session.Add("state", state);
            PropertyBag.Add("state", state);

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(password))
            {
                PropertyBag.Add("returnUrl", returnUrl);
                return;
            }

            FormsAuthentication.Initialize();
            
            var user = userRepository.FindByName(name);

            if (string.IsNullOrEmpty(password) || !user.Password.Match(password))
            {
                Error("Inloggen niet gelukt", true);
                return;
            }

            FormsAuthentication.SetAuthCookie(name, persistent);

            if (string.IsNullOrEmpty(returnUrl))
                RedirectToSiteRoot();
            else
                RedirectToUrl(returnUrl);
        }

        public void ForgotPassword()
        {
            
        }

        public void RequestReset(string email)
        {
            var users = userRepository.FindByEmail(email);
            
            if (users.Count > 0 && !string.IsNullOrEmpty(email))
            {
                users[0].Password.GenerateHash();

                emailService.SendPasswordLink(users[0]);
                userRepository.Save(users[0]);
            }
            else
            {
                PropertyBag.Add("error", "Dit emailadres is niet bekend.");
                RenderView("forgotpassword");
            }
        }

        public void ChangePassword(string hash)
        {
            var users = userRepository.FindByPasswordHash(hash);
            
            if (users.Count > 0 && !string.IsNullOrEmpty(hash))
            {
                PropertyBag.Add("hash", hash);
            }
            else
            {
                PropertyBag.Add("error", "Deze link is verlopen");
            }
        }

        public void SavePassword(string hash, string password, string passwordCheck)
        {
            // password1 omdat 'password' ervoor zorgt dat bij redirect naar inlogpagina, het wachtwoordveld al wordt ingevuld
            if (password != passwordCheck)
            {

                PropertyBag.Add("error", "De wachtwoorden komen niet overeen. Probeer opnieuw.");
                PropertyBag.Add("hash", hash);
                RenderView("changepassword");
            }

            // check if hash (customer) exists
            var users = userRepository.FindByPasswordHash(hash);

            // if hash (customer) exists
            if (users.Count > 0)
            {
                users[0].Password = new Password(password);
                userRepository.Save(users[0]);

                PropertyBag.Add("message", "Uw wachtwoord is met succes gewijzigd. U kunt hieronder inloggen met uw nieuwe wachtwoord.");
                RenderView("Index");
            }
            else
            {
                RedirectToAction("ChangePassword", new Hashtable { { "hash", hash } });
            }

        }

        public void Signout()
        {
            FormsAuthentication.SignOut();
            RedirectToSiteRoot();
        }
    }
}