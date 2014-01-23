namespace Gitle.Clients.GitHub.Models
{
    using System.Runtime.Serialization;
    using Post;

    [DataContract]
    public class Milestone
    {
        [DataMember(Name = "number")]
        public virtual int Number { get; set; }

        [DataMember(Name = "state")]
        public virtual string State { get; set; }

        [DataMember(Name = "title")]
        public virtual string Title { get; set; }

        [DataMember(Name = "description")]
        public virtual string Description { get; set; }

        public virtual MilestonePost ToPost()
        {
            return new MilestonePost
            {
                Title = Title
            };
        }


    }
}