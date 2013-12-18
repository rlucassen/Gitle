using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Clients.Freckle
{
    using Interfaces;
    using Models;
    using ServiceStack.ServiceClient.Web;

    public class ProjectClient : IProjectClient 
    {
        private readonly JsonServiceClient _client;

        public ProjectClient(string domain, string token)
        {
            _client = new JsonServiceClient("https://" + domain + ".letsfreckle.com/api/");
            _client.Headers.Add("X-FreckleToken", token);
        }

        public List<Project> List()
        {
            return _client.Get<List<ProjectResult>>("projects").Select(result => result.Project).ToList();
        }

        public Project Show(int id)
        {
            return _client.Get<ProjectResult>("projects/" + id).Project;
        }

        protected class ProjectResult
        {
            public Project Project { get; set; }
        }

    }
}
