namespace Gitle.Clients.GitHub.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Label
    {
        [DataMember(Name = "name")]
        public virtual string Name { get; set; }

        [DataMember(Name = "color")]
        public virtual string Color { get; set; }
    }
}