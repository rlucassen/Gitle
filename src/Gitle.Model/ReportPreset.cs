namespace Gitle.Model
{
    public class ReportPreset : ModelBase
    {
        public virtual User User { get; set; }
        public virtual string Name { get; set; }
        public virtual string ReportString { get; set; }
    }
}