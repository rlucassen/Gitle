namespace Gitle.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enum;
    using Helpers;
    using Interfaces.Model;

    public class Issue : Touchable
    {
        public Issue()
        {
            Labels = new List<Label>();
            Comments = new List<Comment>();
            ChangeStates = new List<ChangeState>();
            Changes = new List<Change>();
            Pickups = new List<Pickup>();
        }

        public virtual int Number { get; set; }
        public virtual string Name { get; set; }
        public virtual string Body { get; set; }
        public virtual double Hours { get; set; }
        public virtual int Devvers { get; set; }

        public virtual bool Prioritized { get; set; }
        public virtual int PrioOrder { get; set; }

        public virtual Project Project { get; set; }
        public virtual IList<Label> Labels { get; set; }
        public virtual IList<Comment> Comments { get; set; }
        public virtual IList<ChangeState> ChangeStates { get; set; }
        public virtual IList<Change> Changes { get; set; }
        public virtual IList<Pickup> Pickups { get; set; }

        public virtual string BodyHtml { get { return Body.Markdown(Project); } }

        public new virtual bool Touched(User user)
        {
            return Touches.Any(x => x.User == user) && Actions.All(x => x.Touched(user));
        }

        public new virtual bool TouchedBefore(User user, DateTime datetime)
        {
            return Touches.Any(x => x.User == user && x.DateTime < datetime) && Actions.All(x => x.TouchedBefore(user, datetime));
        }

        public virtual bool IsOpen
        {
            get { return State == IssueState.Open; }
        }

        public virtual bool IsArchived
        {
            get { return State == IssueState.Archived; }
        }

        public virtual IssueState State
        {
            get
            {
                return ChangeStates.Any() ? ChangeStates.OrderByDescending(x => x.CreatedAt).First().IssueState : IssueState.Unknown;
            }
        }

        public virtual string StateString
        {
            get
            {
                return State.GetDescription();
            }
        }

        public virtual User PickedUpBy
        {
            get
            {
                var pickup =
                    Pickups.OrderByDescending(x => x.CreatedAt).LastOrDefault();
                return pickup != null ? pickup.User : null;
            }
        }

        public virtual DateTime? PickedUpAt
        {
            get
            {
                var pickup =
                    Pickups.OrderByDescending(x => x.CreatedAt).LastOrDefault();
                return pickup != null ? pickup.CreatedAt : (DateTime?)null;
            }
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

        public virtual User CreatedBy
        {
            get
            {
                var state =
                    ChangeStates.OrderByDescending(x => x.CreatedAt).LastOrDefault(
                        x => x.IssueState == IssueState.Open);
                return state != null ? state.User : null;
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

        public virtual User ClosedBy
        {
            get
            {
                var state =
                    ChangeStates.OrderByDescending(x => x.CreatedAt).FirstOrDefault(
                        x => x.IssueState == IssueState.Closed);
                return state != null ? state.User : null;
            }
        }

        public virtual DateTime? ArchivedAt
        {
            get
            {
                var state =
                    ChangeStates.OrderByDescending(x => x.CreatedAt).FirstOrDefault(
                        x => x.IssueState == IssueState.Archived);
                return state != null ? state.CreatedAt : (DateTime?)null;
            }
        }

        public virtual User ArchivedBy
        {
            get
            {
                var state =
                    ChangeStates.OrderByDescending(x => x.CreatedAt).FirstOrDefault(
                        x => x.IssueState == IssueState.Archived);
                return state != null ? state.User : null;
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
                actions.AddRange(Pickups);
                return actions.OrderByDescending(x => x.CreatedAt).ToList();
            }
        }

        public virtual IList<IIssueAction> ActionsReverse
        {
            get { return Actions.Reverse().ToList(); }
        }

        public virtual string DevversString
        {
            get { return Hours > 0 ? Devvers.ToString() : "n.n.b."; }
        }

        public virtual string HoursString
        {
            get { return Hours > 0 ? Hours <= 3 ? string.Format("{0} uur", Hours) : string.Format("{0} dag", Hours / 8) : "n.n.b."; }
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

        public virtual void ChangeState(User user, IssueState state)
        {
            if(State != state)
                ChangeStates.Add(new ChangeState() { CreatedAt = DateTime.Now, IssueState = state, User = user, Issue = this });
        }

        public virtual void Open(User user)
        {
            if(State != IssueState.Archived)
                ChangeState(user, IssueState.Open);
        }

        public virtual void Close(User user)
        {
            if(State != IssueState.Archived)
                ChangeState(user, IssueState.Closed);
            PrioOrder = 0;
        }

        public virtual void Archive(User user)
        {
            ChangeState(user, IssueState.Archived);
            PrioOrder = 0;
        }

        public virtual void Change(User user)
        {
            Changes.Add(new Change() { CreatedAt = DateTime.Now, User = user, Issue = this});
        }

        public virtual void Pickup(User user)
        {
            Pickups.Add(new Pickup() { CreatedAt = DateTime.Now, User = user, Issue = this });
        }
    }
}