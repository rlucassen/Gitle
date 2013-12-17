using System;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub
{
    using System.Collections.Generic;
    using System.Configuration;
    using Gitle.Clients.GitHub.Models;
    using Interfaces;
    using ServiceStack.ServiceClient.Web;

    public class CommentClient : ICommentClient
    {
        private readonly JsonServiceClient _client;

        public CommentClient(string token, string useragent, string githubApi)
        {
            _client = new JsonServiceClient(githubApi);
            _client.Headers.Add("Authorization", "token " + token);
            _client.LocalHttpWebRequestFilter = request => request.UserAgent = useragent;
        }

        public List<Comment> List(string repo, int issueId)
        {
            return _client.Get<List<Comment>>("repos/" +  repo + "/issues/" + issueId + "/comments");
        }

        public Comment Post(string repo, int issueId, Comment comment)
        {
            return _client.Post<Comment>("repos/" + repo + "/issues/" + issueId + "/comments", comment.ToPost());
        }

    }
}