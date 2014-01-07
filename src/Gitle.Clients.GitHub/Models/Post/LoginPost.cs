namespace Gitle.Clients.GitHub.Models.Post
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LoginPost
    {
        [DataMember(Name = "client_id")]
        public virtual string ClientId { get; set; }

        [DataMember(Name = "client_secret")]
        public virtual string ClientSecret { get; set; }

        [DataMember(Name = "code")]
        public virtual string Code { get; set; }

    }
}