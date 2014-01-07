using System;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub
{
    using System.Collections.Generic;
    using System.Configuration;
    using Gitle.Clients.GitHub.Models;
    using Interfaces;
    using ServiceStack.ServiceClient.Web;

    public class UserClient : IUserClient
    {
        private readonly JsonServiceClient _client;

        public UserClient(string token, string useragent, string githubApi)
        {
            _client = new JsonServiceClient(githubApi);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = useragent;
        }

        public User Get(string accessToken)
        {
            return _client.Get<User>("user?access_token=" + accessToken);
        }
    }
}