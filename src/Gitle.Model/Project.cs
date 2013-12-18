namespace Gitle.Model
{
    using System.Collections.Generic;
    using Helpers;

    public class Project : ModelBase
    {
        private string name;
        public virtual string Name
        {
            get { return name; }
            set { 
                name = value;
                Slug = string.IsNullOrEmpty(value) ? string.Empty : SlugHelper.Slugify(value);
            }
        }

        public virtual string Slug { get; set; }

        public virtual string Repository { get; set; }
        public virtual int MilestoneId { get; set; }
        public virtual string MilestoneName { get; set; }
        public virtual int HourPrice { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual int FreckleId { get; set; }
        public virtual IList<UserProject> Users { get; set; }

    }
}