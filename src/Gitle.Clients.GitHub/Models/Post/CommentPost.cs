namespace Gitle.Clients.GitHub.Models.Post
{
    using System.Runtime.Serialization;

    [DataContract]
    public class CommentPost
    {
        [DataMember(Name = "body")]
        public virtual string Body { get; set; }
    }
}