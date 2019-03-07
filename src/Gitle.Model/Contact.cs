namespace Gitle.Model
{
    using System.Collections.Generic;
    using Interfaces.Model;

    public class Contact : ModelBase
    {
        public virtual string FullName { get; set; }
        public virtual string Email { get; set; }
        public virtual string PhoneNumber { get; set; }
    }
}