namespace Gitle.Clients.GitHub.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class User : IEquatable<User>
    {
        [DataMember(Name = "id")]
        public virtual int Id { get; set; }

        [DataMember(Name = "login")]
        public virtual string Login { get; set; }

        [DataMember(Name = "name")]
        public virtual string Name{ get; set; }

        [DataMember(Name = "email")]
        public virtual string Email { get; set; }

        public virtual bool Equals(User other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id == Id && Equals(other.Login, Login);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (User)) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id*397) ^ (Login != null ? Login.GetHashCode() : 0);
            }
        }
    }
}