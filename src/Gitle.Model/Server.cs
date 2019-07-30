namespace Gitle.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Model;

    public class Server : ModelBase, ISlugger
    {
        public virtual string Name { get; set; }
        public virtual string Slug { get; set; }
        public virtual Hosting Hosting { get; set; }
        public virtual string Description { get; set; }
        public virtual bool HaveAccessToServer { get; set; } 
        public virtual IList<Installation> Installations { get; set; } = new List<Installation>();

        public virtual IList<Application> Applications => Installations.Select(x => x.Application).Distinct().ToList();
    }
}