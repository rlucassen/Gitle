using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Gitle.Clients.GitHub.Models
{
    [DataContract]
    public class Hook
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

        [DataMember(Name = "events")]
        public List<string> Events { get; set; }

        [DataMember(Name = "config")]
        public Dictionary<string, string> Config { get; set; }

        public Hook(string url)
        {
            Config = new Dictionary<string, string> {{"url", url}, {"content_type", "form"}, {"insecure_ssl", "1"}};
            Events = new List<string> {"issue_comment", "issues"};
            Active = true;
            Name = "web";
        }
    }
}