namespace Gitle.Clients.GitHub
{
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
            _client.Headers.Add("Authorization", "token " + accessToken);
            return _client.Get<User>("user");
        }
    }
}