using System;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub
{
    using System.Collections.Generic;
    using System.Configuration;
    using Gitle.Clients.GitHub.Models;
    using Interfaces;
    using ServiceStack.ServiceClient.Web;

    public class LabelClient : ILabelClient
    {
        private readonly JsonServiceClient _client;

        public LabelClient(string token, string useragent, string githubApi)
        {
            _client = new JsonServiceClient(githubApi);
            _client.Headers.Add("Authorization", "token " + token);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = useragent;
        }

        public List<Label> List(string repo)
        {
            return _client.Get<List<Label>>("repos/" + repo + "/labels");
        }

        public Label Get(string repo, string labelId)
        {
            return _client.Get<Label>("repos/" + repo + "/labels/" + labelId);
        }

        public Label Post(string repo, Label label)
        {
            return _client.Post<Label>("repos/" + repo + "/labels", label);
        }

        public Label Patch(string repo, string labelId, Label label)
        {
            return _client.Patch<Label>("repos/" + repo + "/labels/" + labelId, label);
        }

        public List<Label> AddLabelToIssue(string repo, int issueId, Label label)
        {
            return _client.Post<List<Label>>("repos/" + repo + "/issues/" + issueId + "/labels", new[] {label.Name});
        }

    }
}