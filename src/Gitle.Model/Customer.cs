namespace Gitle.Model
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Model;

    public class Customer : ModelBase, ISlugger
    {
        public Customer()
        {
            Contacts = new List<User>();
            Projects = new List<Project>();
        }

        public virtual string Name { get; set; }
        public virtual string Slug { get; set; }
        public virtual IList<User> Contacts { get; set; }
        public virtual IList<Project> Projects { get; set; }
        public virtual string Comments { get; set; }

        public virtual IList<User> AllContacts
        {
            get
            {
                var allProjectContacts = Projects.SelectMany(p => p.Users.Select(u => u.User).Where(u => !u.IsAdmin)).ToList();
                allProjectContacts.AddRange(Contacts);
                return allProjectContacts.Distinct().ToList();
            }
        }
    }
}