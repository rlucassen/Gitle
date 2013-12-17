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
            var issues = _client.Get<List<Issue>>("repos/" + repo + "/issues");
            issues.AddRange(_client.Get<List<Issue>>("repos/" + repo + "/issues?state=closed"));
            return issues;
        }

        public List<Issue> List(string repo, int milestoneId)
        {
            var issues = _client.Get<List<Issue>>("repos/" + repo + "/issues?milestone=" + milestoneId);
            issues.AddRange(_client.Get<List<Issue>>("repos/" + repo + "/issues?state=closed&milestone=" + milestoneId));
            return issues;
        }

        public Issue Get(string repo, int issueId)
        {
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