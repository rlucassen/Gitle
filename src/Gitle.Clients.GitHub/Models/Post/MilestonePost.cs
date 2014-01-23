namespace Gitle.Clients.GitHub.Models.Post
{
    using System.Runtime.Serialization;

    [DataContract]
    public class MilestonePost
    {
        [DataMember(Name = "title")]
        public virtual string Title { get; set; }
    }
}