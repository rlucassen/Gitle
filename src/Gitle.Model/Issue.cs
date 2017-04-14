﻿namespace Gitle.Model
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
            Bookings = new List<Booking>();
            InvoiceLines = new List<InvoiceLine>();
            EstimatePublic = true;
        }

        public virtual int Number { get; set; }
        public virtual string Name { get; set; }
        public virtual string Body { get; set; }
        public virtual double Hours { get; set; }
        public virtual int Devvers { get; set; }
        public virtual bool EstimatePublic { get; set; }

        public virtual bool Prioritized { get; set; }
        public virtual int PrioOrder { get; set; }

        public virtual Project Project { get; set; }
        public virtual IList<Label> Labels { get; set; }
        public virtual IList<Comment> Comments { get; set; }
        public virtual IList<ChangeState> ChangeStates { get; set; }
        public virtual IList<Change> Changes { get; set; }
        public virtual IList<Pickup> Pickups { get; set; }
        public virtual IList<Booking> Bookings { get; set; }
        public virtual IList<InvoiceLine> InvoiceLines { get; set; }

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

        public virtual bool HasBeenOpenSince(DateTime dateTime)
        {
            if (!ChangeStates.Any()) return false;
            var oldStates = ChangeStates.Where(x => x.CreatedAt <= dateTime);
            var wasOpenAt = oldStates.Any() ? oldStates.OrderByDescending(x => x.CreatedAt).First().IssueState == IssueState.Open : false;
            var hasBeenOpenSince = ChangeStates.Any(x => x.CreatedAt >= dateTime && x.IssueState == IssueState.Open);
            return wasOpenAt || hasBeenOpenSince;
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
            get { return Hours > 0 ? Hours.ToHourDayNotation() : "n.n.b."; }
        }

        public virtual string EstimateString
        {
            get { return Hours > 0 ? $"{Devvers} developer{(Devvers > 1 ? "s" : "")} {HoursString}" : "n.n.b."; }
        }

        public virtual double TotalHours
        {
            get { return Hours * Devvers; }
        }

        public virtual string TotalHoursString
        {
            get { return TotalHours > 0 ? TotalHours.ToHourDayNotation() : "n.n.b."; }
        }

        public virtual IList<Invoice> Invoices
        {
            get { return InvoiceLines.Select(x => x.Invoice).ToList(); }
        }

        public virtual IList<Invoice> DefinitiveInvoices
        {
            get { return InvoiceLines.Select(x => x.Invoice).Where(i => i.IsDefinitive).ToList(); }
        }

        public virtual double TotalHoursInvoiced
        {
            get { return InvoiceLines.Where(x => x.Invoice.IsDefinitive).Sum(x => x.Hours); }
        }
        public virtual string TotalHoursInvoicedString => TotalHoursInvoiced.ToHourDayNotation();

        public virtual double TotalBillableHoursInvoiced
        {
            get { return InvoiceLines.Where(x => x.Invoice.IsDefinitive && !x.Null).Sum(x => x.Hours); }
        }
        public virtual string TotalBillableHoursInvoicedString => TotalBillableHoursInvoiced.ToHourDayNotation();

        public virtual double TotalUnbillableHoursInvoiced
        {
            get { return InvoiceLines.Where(x => x.Invoice.IsDefinitive && x.Null).Sum(x => x.Hours); }
        }
        public virtual string TotalUnbillableHoursInvoicedString => TotalUnbillableHoursInvoiced.ToHourDayNotation();

        public virtual string CostString(decimal hourPrice)
        {
            return TotalHours > 0 ? ((decimal)TotalHours * hourPrice).ToString("C") : "n.n.b.";
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

        public virtual double BillableBookingHours()
        {
            return Bookings.Where(x => x.IsActive && !x.Unbillable).Sum(x => x.Hours);
        }

        public virtual string BillableBookingHoursString()
        {
            return BillableBookingHours().ToHourDayNotation();
        }

        public virtual double UnbillableBookingHours()
        {
            return Bookings.Where(x => x.IsActive && x.Unbillable).Sum(x => x.Hours);
        }

        public virtual string UnbillableBookingHoursString()
        {
            return UnbillableBookingHours().ToHourDayNotation();
        }

        public virtual double BookingHours(DateTime startDate, DateTime endDate)
        {
            return Bookings.Where(x => x.Date >= startDate && x.Date <= endDate).Sum(x => x.Hours);
        }

        public virtual double MaxOfBookingAndTotalHours()
        {
            return Math.Max(TotalHours, BillableBookingHours());
        }

        public virtual string ShortName()
        {
            if (Name != null)
            {
                if (Name.Length > 43)
                {
                    return Name.Substring(0, 40) + "...";
                }
                else
                {
                    return Name;
                }
            }
            else
            {
                return "ERROR - Issue heeft geen naam!";
            }
        }
    }
}