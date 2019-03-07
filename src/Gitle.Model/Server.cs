namespace Gitle.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Model;

    public class Server : ModelBase, ISlugger
    {
        public virtual string Name { get; set; }
        public virtual string Slug { get; set; }
        public virtual string HostingCompany { get; set; }
        public virtual bool IsExternal { get; set; }
        public virtual IList<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual IList<Installation> Installations { get; set; } = new List<Installation>();
        public virtual string Description { get; set; }

        public virtual IList<Application> Applications => Installations.Select(x => x.Application).Distinct().ToList();
    }
}