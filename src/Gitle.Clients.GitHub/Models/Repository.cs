namespace Gitle.Clients.GitHub.Models
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class Repository : IEquatable<Repository>
    {
        [DataMember(Name = "id")]
        public virtual int Id { get; set; }

        [DataMember(Name = "name")]
        public virtual string Name { get; set; }

        [DataMember(Name = "full_name")]
        public virtual string FullName { get; set; }

        [DataMember(Name = "description")]
        public virtual string DescriptionId { get; set; }

        public virtual List<Milestone> Milestones { get; set; }

        [DataMember(Name = "owner")]
        public virtual User Owner { get; set; }

        public virtual bool Equals(Repository other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id == Id && Equals(other.Name, Name) && Equals(other.FullName, FullName) && Equals(other.DescriptionId, DescriptionId) && Equals(other.Owner, Owner);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Repository)) return false;
            return Equals((Repository) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = Id;
                result = (result*397) ^ (Name?.GetHashCode() ?? 0);
                result = (result*397) ^ (FullName?.GetHashCode() ?? 0);
                result = (result*397) ^ (DescriptionId?.GetHashCode() ?? 0);
                result = (result*397) ^ (Owner?.GetHashCode() ?? 0);
                return result;
            }
        }
    }
}