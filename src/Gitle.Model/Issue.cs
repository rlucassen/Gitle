namespace Gitle.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Issue : ModelBase
    {
        public Issue()
        {
            Labels = new List<Label>();
            Comments = new List<Comment>();
        }

        public virtual int Number { get; set; }
        public virtual string Name { get; set; }
        public virtual string State { get; set; }
        public virtual string Body { get; set; }
        public virtual double Hours { get; set; }
        public virtual int Devvers { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? ClosedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Project Project { get; set; }
        public virtual IList<Label> Labels { get; set; }
        public virtual IList<Comment> Comments { get; set; }

        public virtual string DevversString
        {
            get { return Hours > 0 ? Devvers.ToString() : "n.n.b."; }
        }

        public virtual string HoursString
        {
            get { return Hours > 0 ? Hours <= 2.5 ? string.Format("{0} uur", Hours) : string.Format("{0} dag", Hours / 8) : "n.n.b."; }
        }

        public virtual string EstimateString
        {
            get { return Hours > 0 ? string.Format("{0} developer{1} {2}", Devvers, Devvers > 1 ? "s" : "", HoursString) : "n.n.b."; }
        }

        public virtual double TotalHours
        {
            get { return Hours * Devvers; }
        }

        public virtual string CostString(double hourPrice)
        {
            return TotalHours > 0 ? (TotalHours * hourPrice).ToString("C") : "n.n.b.";
        }

        public virtual bool CheckLabel(string label)
        {
            return Labels.Select(l => l.Name).Contains(label);
        }
    }
}