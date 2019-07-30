namespace Gitle.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Model;

    public class Hosting : ModelBase, ISlugger
    {
        public virtual string Name { get; set; }
        public virtual string Website { get; set; }
        public virtual string Phone { get; set; }
        public virtual string SupportMail { get; set; }
        public virtual string BillingMail { get; set; }
        public virtual IList<Contact> Contacts { get; set; } = new List<Contact>(); 
        public virtual string Description { get; set; }
        public virtual string Slug { get; set; }

    }
}