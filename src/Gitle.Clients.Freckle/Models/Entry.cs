namespace Gitle.Clients.Freckle.Models
{
    using System.Runtime.Serialization;

    [DataContract(Name = "entry")]
    public class Entry
    {
        [DataMember(Name = "minutes")]
        public virtual string Minutes { get; set; }

        [DataMember(Name = "user")]
        public virtual string User { get; set; }

        [DataMember(Name = "project-id")]
        public virtual int ProjectId { get; set; }

        [DataMember(Name = "description")]
        public virtual string Description { get; set; }

        [DataMember(Name = "date")]
        public virtual string Date { get; set; }
    }
}