namespace Gitle.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Model;

    public class Project : ModelBase, IDocumentContainer
    {
        public Project()
        {
            Users = new List<UserProject>();
            Labels = new List<Label>();
            Documents = new List<Document>();
        }

        public virtual string Name { get; set; }

        public virtual string Slug { get; set; }

        public virtual string Repository { get; set; }
        public virtual int MilestoneId { get; set; }
        public virtual string MilestoneName { get; set; }
        public virtual int HourPrice { get; set; }
        public virtual int FreckleId { get; set; }
        public virtual string FreckleName { get; set; }
        public virtual string Information { get; set; }
        public virtual string Comments { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual IList<UserProject> Users { get; set; }
        public virtual IList<Label> Labels { get; set; }
        public virtual IList<Issue> Issues { get; set; }
        public virtual IList<Document> Documents { get; set; } 

        public virtual int NotifiedUsers { get { return Users.Count(u => u.Notifications); } }

        public virtual int NewIssueNumber { get { return (Issues.Any() ? Issues.Max(x => x.Number) : 0) + 1; } }
    }
}