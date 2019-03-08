namespace Gitle.Model
{
    using Gitle.Model.Enum;
    using Gitle.Model.Interfaces.Model;


    public class Installation : ModelBase, ISlugger
    {
        public virtual string Name { get; set; }
        public virtual string Slug { get; set; }
        public virtual Application Application { get; set; }
        public virtual InstallationType InstallationType { get; set; }
        public virtual Server Server { get; set; }
        public virtual string Url { get; set; }
        public virtual string Description { get; set; }
    }
}