namespace Gitle.Model
{
    public class FilterPreset : ModelBase
    {
        public virtual User User { get; set; }
        public virtual string Name { get; set; }
        public virtual string FilterString { get; set; }
    }
}