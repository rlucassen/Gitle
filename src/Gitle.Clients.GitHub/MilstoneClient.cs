using System;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub
{
    using System.Collections.Generic;
    using System.Configuration;
    using Gitle.Clients.GitHub.Models;
    using Interfaces;
    using ServiceStack.ServiceClient.Web;

    public class MilestoneClient : IMilestoneClient
    {
        private readonly JsonServiceClient _client;

        public MilestoneClient(string token, string useragent, string githubApi)
        {
            _client = new JsonServiceClient(githubApi);
            _client.Headers.Add("Authorization", "token " + token);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = useragent;
        }

        public List<Milestone> List(string repo)
        {
            return _client.Get<List<Milestone>>("repos/" + repo + "/milestones");
        }

        public Milestone Get(string repo, int milestoneId)
        {
            return _client.Get<Milestone>("repos/" + repo + "/milestones/" + milestoneId);
        }

        public Milestone Post(string repo, Milestone milestone)
        {
            return _client.Post<Milestone>("repos/" + repo + "/milestones", milestone.ToPost());
        }

    }
}