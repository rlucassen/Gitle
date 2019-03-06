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
        public virtual IList<User> Contacts { get; set; } //Mogelijk dat de contacts niet in het systeem zitten?
        public virtual IList<Installation> Installations { get; set; }
        public virtual string Description { get; set; }

        public virtual IList<Application> AllApplications
        {
            get
            {
                //Methode maken om alle applicaties op te halen die op deze server staan (via installations) dubbele dus filteren.
                return null;
            }
        }
    }
}