using System;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub
{
    using System.Collections.Generic;
    using System.Configuration;
    using Gitle.Clients.GitHub.Models;
    using Interfaces;
    using Models.Post;
    using ServiceStack.ServiceClient.Web;

    public class OAuthClient : IOAuthClient
    {
        private readonly JsonServiceClient _client;

        public OAuthClient(string token, string useragent, string githubApi)
        {
            _client = new JsonServiceClient(githubApi);
            _client.Headers.Add("Authorization", "token " + token);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = useragent;
        }

        public AccessToken Login(string code)
        {
            try
            {
                var clientId = ConfigurationManager.AppSettings["githubClientId"];
                var clientSecret = ConfigurationManager.AppSettings["githubClientSecret"];
                return _client.Post<AccessToken>("https://github.com/login/oauth/access_token",
                                                 new LoginPost { ClientId = clientId, ClientSecret = clientSecret, Code = code });
            }
            catch (Exception)
            {
                return new AccessToken();
            }
        }
    }
}