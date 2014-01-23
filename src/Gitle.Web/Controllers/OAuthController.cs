namespace Gitle.Web.Controllers
{
    using System.Web.Security;
    using Castle.MonoRail.Framework;
    using Clients.GitHub.Interfaces;
    using Model.Interfaces.Repository;
    using Model.Interfaces.Service;

    public class OAuthController : BaseController
    {
        private readonly IUserRepository userRepository;
        private readonly IOAuthClient oauthClient;
        private readonly IUserClient userClient;

        public OAuthController(IUserRepository userRepository, IOAuthClient oauthClient, IUserClient userClient)
        {
            this.userRepository = userRepository;
            this.userClient = userClient;
            this.oauthClient = oauthClient;
        }

        public void Index(string code, string state)
        {
            var sessionState = Session["state"];
            if (state.Equals(sessionState))
            {
                var accessToken = oauthClient.Login(code);
                if (!string.IsNullOrEmpty(accessToken.Token))
                {
                    var user = userClient.Get(accessToken.Token);
                    var gitleUsers = userRepository.FindByGithubUser(user.Login);
                    if (gitleUsers.Count > 0)
                    {
                        gitleUsers[0].GitHubAccessToken = accessToken.Token;
                        userRepository.Save(gitleUsers[0]);

                        FormsAuthentication.Initialize();
                        FormsAuthentication.SetAuthCookie(gitleUsers[0].Name, true);
                        RedirectToSiteRoot();
                    }
                    else
                    {
                        RenderView("nogitleuser");
                    }
                }
                else
                {
                    RenderView("codeexpired");
                }
            }
            else
            {
                RenderView("stateincorrect");
            }
        }
    }
}