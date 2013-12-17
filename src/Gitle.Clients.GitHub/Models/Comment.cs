namespace Gitle.Clients.GitHub.Models
{
    using System.Runtime.Serialization;
    using Post;

    [DataContract]
    public class Comment
    {
        [DataMember(Name = "id")]
        public virtual int Id { get; set; }

        [DataMember(Name = "body")]
        public virtual string Body { get; set; }

        public virtual CommentPost ToPost()
        {
            return new CommentPost
                       {
                           Body = Body
                       };
        }
    }
}