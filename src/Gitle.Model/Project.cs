namespace Gitle.Model
{
    public class Project : ModelBase
    {
        public virtual string Name { get; set; }
        public virtual string Repository { get; set; }
        public virtual int Milestone { get; set; }
        public virtual Customer Customer { get; set; }
    }
}