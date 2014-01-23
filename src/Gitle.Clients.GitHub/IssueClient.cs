using System;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub
{
    using System.Collections.Generic;
    using System.Configuration;
    using Gitle.Clients.GitHub.Models;
    using Interfaces;
    using ServiceStack.ServiceClient.Web;

    public class IssueClient : IIssueClient
    {
        private readonly JsonServiceClient _client;

        public IssueClient(string token, string useragent, string githubApi)
        {
            _client = new JsonServiceClient(githubApi);
            _client.Headers.Add("Authorization", "token " + token);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = useragent;
        }

        public List<Issue> List(string repo)
        {
            var issues = _client.Get<List<Issue>>("repos/" + repo + "/issues?per_page=100");
            issues.AddRange(_client.Get<List<Issue>>("repos/" + repo + "/issues?per_page=100&state=closed"));
            return issues;
        }

        public List<Issue> List(string repo, int milestoneId, string state = "open,closed")
        {

            var issues = new List<Issue>();
            foreach (var s in state.Split(','))
            {
                issues.AddRange(_client.Get<List<Issue>>("repos/" + repo + "/issues?per_page=100&state=" + s + "&milestone=" + milestoneId));
            }
            return issues;
        }

        public Issue Get(string repo, int issueId)
        {
            if (issueId == 0) return null;
            return _client.Get<Issue>("repos/" + repo + "/issues/" + issueId);
        }

        public Issue Post(string repo, Issue issue)
        {
            return _client.Post<Issue>("repos/" + repo + "/issues", issue.ToPost());
        }

        public Issue Patch(string repo, int issueId, Issue issue)
        {
            return _client.Patch<Issue>("repos/" + repo + "/issues/" + issueId, issue.ToPatch());
        }
    }
}