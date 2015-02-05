namespace Gitle.Web.Controllers
{
    #region Usings

    using System.Collections;
    using System.Configuration;
    using System.Linq;
    using System.Web.Security;
    using Model;
    using Model.Helpers;
    using Model.Interfaces.Service;
    using Model.Nested;
    using NHibernate;

    #endregion

    public class AuthenticationController : BaseController
    {
        private readonly ISession session;
        private readonly IEmailService emailService;

        public AuthenticationController(ISessionFactory sessionFactory, IEmailService emailService)
        {
            this.session = sessionFactory.GetCurrentSession();
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

            var users = session.QueryOver<User>().Where(x => x.IsActive).And(x => x.Name == name).List();
            var user = users.Count > 0 ? users.First() : new User.NullUser();

            if (user is User.NullUser || string.IsNullOrEmpty(password) || !user.Password.Match(password))
            {
                Error("De gebruikersnaam of het wachtwoord is onjuist.", true);
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
            var users = session.QueryOver<User>().Where(x => x.IsActive).And(x => x.EmailAddress == email).List();
            
            if (users.Count > 0 && !string.IsNullOrEmpty(email))
            {
                users[0].Password.GenerateHash();

                emailService.SendPasswordLink(users[0]);
                using (var tx = session.BeginTransaction())
                {
                    session.SaveOrUpdate(users[0]);
                    tx.Commit();
                }
            }
            else
            {
                PropertyBag.Add("error", "Dit emailadres is niet bekend.");
                RenderView("forgotpassword");
            }
        }

        public void ChangePassword(string hash)
        {
            var users = session.QueryOver<User>().Where(x => x.IsActive).And(x => x.Password.Hash == hash).List();
            
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
            var users = session.QueryOver<User>().Where(x => x.IsActive).And(x => x.Password.Hash == hash).List();

            // if hash (customer) exists
            if (users.Count > 0)
            {
                users[0].Password = new Password(password);
                using (var tx = session.BeginTransaction())
                {
                    session.SaveOrUpdate(users[0]);
                    tx.Commit();
                }

                Flash.Add("message", "Uw wachtwoord is met succes gewijzigd. U kunt hieronder inloggen met uw nieuwe wachtwoord.");
                RenderView("index");
                Index("", "", false, "");
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