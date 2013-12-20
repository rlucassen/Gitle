using System;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub
{
    using System.Collections.Generic;
    using System.Configuration;
    using Gitle.Clients.GitHub.Models;
    using Interfaces;
    using ServiceStack.ServiceClient.Web;

    public class RepositoryClient : IRepositoryClient
    {
        private readonly JsonServiceClient _client;

        public RepositoryClient(string token, string useragent, string githubApi)
        {
            _client = new JsonServiceClient(githubApi);
            _client.Headers.Add("Authorization", "token " + token);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = useragent;
        }

        public List<Repository> List()
        {
            return _client.Get<List<Repository>>("user/repos?type=owner&per_page=100");
        }

        public Repository Get(string repo)
        {
            return _client.Get<Repository>("repos/" + repo);
        }

        public IList<Hook> GetHooks(string repo)
        {
            try
            {
                return _client.Get<List<Hook>>("repos/" + repo + "/hooks");
            }
            catch(Exception e)
            {
                return new List<Hook>();
            }
        }

        public bool PostHook(string repo, string url)
        {
            try
            {
                _client.Post<Hook>("repos/" + repo + "/hooks", new Hook(url));
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }
    }
}