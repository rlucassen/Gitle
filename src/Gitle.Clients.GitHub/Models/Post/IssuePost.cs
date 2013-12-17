namespace Gitle.Clients.GitHub.Models.Post
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class IssuePost
    {
        [DataMember(Name = "title")]
        public virtual string Title { get; set; }

        [DataMember(Name = "body")]
        public virtual string Body { get; set; }

        [DataMember(Name = "milestone")]
        public virtual int Milestone { get; set; }

        [DataMember(Name = "labels")]
        public virtual List<string> Labels { get; set; }
    }
}