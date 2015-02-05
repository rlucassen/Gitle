namespace Gitle.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enum;
    using Interfaces.Model;

    public class Issue : ModelBase
    {
        public Issue()
        {
            Labels = new List<Label>();
            Comments = new List<Comment>();
            ChangeStates = new List<ChangeState>();
            Changes = new List<Change>();
        }

        public virtual int Number { get; set; }
        public virtual string Name { get; set; }
        public virtual string State { get; set; }
        public virtual string Body { get; set; }
        public virtual double Hours { get; set; }
        public virtual int Devvers { get; set; }

        public virtual Project Project { get; set; }
        public virtual IList<Label> Labels { get; set; }
        public virtual IList<Comment> Comments { get; set; }
        public virtual IList<ChangeState> ChangeStates { get; set; }
        public virtual IList<Change> Changes { get; set; }

        public virtual bool Open
        {
            get { return ChangeStates.OrderByDescending(x => x.CreatedAt).First().IssueState != IssueState.Closed; }
        }

        public virtual DateTime? CreatedAt
        {
            get
            {
                var state =
                    ChangeStates.OrderByDescending(x => x.CreatedAt).LastOrDefault(
                        x => x.IssueState == IssueState.Open);
                return state != null ? state.CreatedAt : (DateTime?)null;
            }
        }

        public virtual DateTime? ClosedAt
        {
            get
            {
                var state =
                    ChangeStates.OrderByDescending(x => x.CreatedAt).FirstOrDefault(
                        x => x.IssueState == IssueState.Closed);
                return state != null ? state.CreatedAt : (DateTime?)null;
            }
        }

        public virtual DateTime? UpdatedAt
        {
            get
            {
                var state =
                    Changes.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                return state != null ? state.CreatedAt : (DateTime?)null;
            }
        }

        public virtual IList<IIssueAction> Actions
        {
            get
            {
                var actions = new List<IIssueAction>(Comments);
                actions.AddRange(ChangeStates);
                actions.AddRange(Changes);
                return actions.OrderBy(x => x.CreatedAt).ToList();
            }
        }

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

        public virtual void Close(User user)
        {
            ChangeStates.Add(new ChangeState(){CreatedAt = DateTime.Now, IssueState = IssueState.Closed, User = user});
        }

        public virtual void Reopen(User user)
        {
            ChangeStates.Add(new ChangeState() { CreatedAt = DateTime.Now, IssueState = IssueState.Open, User = user });
        }

        public virtual void Change(User user)
        {
            Changes.Add(new Change() { CreatedAt = DateTime.Now, User = user });
        }
    }
}