namespace Gitle.Web.Controllers
{
    using System.Web.Security;
    using Clients.GitHub.Interfaces;
    using Model;
    using NHibernate;

    public class OAuthController : BaseController
    {
        private readonly IOAuthClient oauthClient;
        private readonly IUserClient userClient;

        public OAuthController(ISessionFactory sessionFactory, IOAuthClient oauthClient, IUserClient userClient) : base(sessionFactory)
        {
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
                    var gitleUsers = session.QueryOver<User>().Where(x => x.IsActive).And(x => x.GitHubUsername == user.Login).List();

                    if (gitleUsers.Count > 0)
                    {
                        gitleUsers[0].GitHubAccessToken = accessToken.Token;
                        using (var tx = session.BeginTransaction())
                        {
                            session.SaveOrUpdate(gitleUsers[0]);
                            tx.Commit();
                        }

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