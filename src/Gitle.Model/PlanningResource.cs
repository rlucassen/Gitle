namespace Gitle.Model
{
    using System.Collections.Generic;

    public class PlanningResource : ModelBase
    {
        public virtual int Year { get; set; }
        public virtual int Week { get; set; }
        public virtual Project Project { get; set; }
        public virtual IList<Issue> Issues { get; set; }
    }
}