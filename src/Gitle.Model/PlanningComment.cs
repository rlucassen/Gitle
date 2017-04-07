namespace Gitle.Model
{
    using Interfaces.Model;
    public class PlanningComment : ModelBase, ISlugger
    {
        public virtual string Slug { get; set; }
        public virtual string Comment { get; set; }
        public virtual User User { get; set; }
    }
}