namespace Gitle.Model
{
    using System.Collections.Generic;

    public class Application : ModelBase
    {
        public virtual string Name { get; set; }
        public virtual string Slug { get; set; }
        public virtual string Comments { get; set; }
        public virtual IList<Project> Projects { get; set; }
        public virtual Customer Customer { get; set; }
    }
}