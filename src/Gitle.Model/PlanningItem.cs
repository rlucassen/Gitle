namespace Gitle.Model
{
    using System;

    public class PlanningItem : ModelBase
    {
        public virtual User User { get; set; }
        public virtual Project Project { get; set; }
        public virtual DateTime Start { get; set; }
        public virtual DateTime End { get; set; }
    }
}