namespace Gitle.Model
{
    using System.Collections.Generic;
    using Helpers;

    public class Project : ModelBase
    {
        public Project()
        {
            Users = new List<UserProject>();
            Labels = new List<Label>();
        }

        public virtual string Name { get; set; }

        public virtual string Slug { get; set; }

        public virtual string Repository { get; set; }
        public virtual int MilestoneId { get; set; }
        public virtual string MilestoneName { get; set; }
        public virtual int HourPrice { get; set; }
        public virtual int FreckleId { get; set; }
        public virtual string FreckleName { get; set; }
        public virtual IList<UserProject> Users { get; set; }
        public virtual IList<Label> Labels { get; set; }
        public virtual string Information { get; set; }

    }
}