namespace Gitle.Clients.GitHub.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AccessToken
    {
        [DataMember(Name = "access_token")]
        public virtual string Token { get; set; }

        [DataMember(Name = "scope")]
        public virtual string Scope { get; set; }

        [DataMember(Name = "token_type")]
        public virtual string TokenType { get; set; }
    }
}