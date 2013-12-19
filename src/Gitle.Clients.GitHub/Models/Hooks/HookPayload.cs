namespace Gitle.Clients.GitHub.Models.Hooks
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract]
    public class HookPayload
    {
        [DataMember(Name = "action")]
        public virtual string Action { get; set; }

        [DataMember(Name = "comment")]
        public virtual Comment Comment { get; set; }

        [DataMember(Name = "issue")]
        public virtual Issue Issue { get; set; }
    }
}