namespace Gitle.Clients.Freckle.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class Project : IEquatable<Project>
    {
        // ID of the project (integer)
        [DataMember(Name = "id")]
        public virtual int Id { get; set; }
        // Name of the project
        [DataMember(Name = "name")]
        public virtual string Name { get; set; }
        // Group name of the project (or null)
        [DataMember(Name = "group_name")]
        public virtual string GroupName { get; set; }
        // Group ID of the project (or null)
        [DataMember(Name = "project_group_id")]
        public virtual int? ProjectGroupId { get; set; }
        // UTC timestamp when the project was created
        [DataMember(Name = "created_at")]
        public virtual DateTime CreatedAt { get; set; }
        // UTC timestamp when the project was last updated
        [DataMember(Name = "updated_at")]
        public virtual DateTime UpdatedAt { get; set; }
        // total amount of logged minutes (can be null)
        [DataMember(Name = "minutes")]
        public virtual int? Minutes { get; set; }
        // total amount of logged billable minutes (can be null)
        [DataMember(Name = "billable_minutes")]
        public virtual int? BillableMinutes { get; set; }
        // total amount of unbillable minutes (can be null)
        [DataMember(Name = "unbillable_minutes")]
        public virtual int? UnbillableMinutes { get; set; }
        // total amount of logged invoiced minutes (can be null)
        [DataMember(Name = "invoiced_minutes")]
        public virtual int? InvoicedMinutes { get; set; }
        // amount of remaining minutes within budget (can be null)
        [DataMember(Name = "remaining_minutes")]
        public virtual int? RemainingMinutes { get; set; }
        // budgeted minutes (can be null)
        [DataMember(Name = "budget_minutes")]
        public virtual int? BudgetMinutes { get; set; }
        // true if project is billable
        [DataMember(Name = "billable")]
        public virtual bool Billable { get; set; }
        // ID of the import if the project was part of an import (can be null)
        [DataMember(Name = "import_id")]
        public virtual int? ImportId { get; set; }
        // true if project is enabled, false if archived
        [DataMember(Name = "enabled")]
        public virtual bool Enabled { get; set; }
        // rrggbb hexadecimal color for the project
        [DataMember(Name = "color_hex")]
        public virtual string ColorHex { get; set; }
        // billing increment in minutes
        // if you build client software that creates entries
        // it should only allow multiples of stepping logged
        // (in minutes). if the user enters a non-multiple you
        // should round time up.
        [DataMember(Name = "stepping")]
        public virtual int Stepping { get; set; }

        // all following fields are deprecated, and may 
        // be removed in the next API version
        [DataMember(Name = "user_id")]
        public virtual int? UserId { get; set; }
        [DataMember(Name = "budget")]
        public virtual int? Budget { get; set; }
        [DataMember(Name = "account_id")]
        public virtual int? AccountId { get; set; }
        [DataMember(Name = "invoice_recipient_details")]
        public virtual string InvoiceRecipientDetails { get; set; }
        [DataMember(Name = "cached_tags")]
        public virtual string[] CachedTags { get; set; }

        public virtual bool Equals(Project other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id == Id && Equals(other.Name, Name) && Equals(other.GroupName, GroupName) && other.ProjectGroupId.Equals(ProjectGroupId) && other.CreatedAt.Equals(CreatedAt) && other.UpdatedAt.Equals(UpdatedAt) && other.Minutes.Equals(Minutes) && other.BillableMinutes.Equals(BillableMinutes) && other.UnbillableMinutes.Equals(UnbillableMinutes) && other.InvoicedMinutes.Equals(InvoicedMinutes) && other.RemainingMinutes.Equals(RemainingMinutes) && other.BudgetMinutes.Equals(BudgetMinutes) && other.Billable.Equals(Billable) && other.ImportId.Equals(ImportId) && other.Enabled.Equals(Enabled) && Equals(other.ColorHex, ColorHex) && other.Stepping == Stepping && other.UserId.Equals(UserId) && other.Budget.Equals(Budget) && other.AccountId.Equals(AccountId) && Equals(other.InvoiceRecipientDetails, InvoiceRecipientDetails) && Equals(other.CachedTags, CachedTags);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Project)) return false;
            return Equals((Project)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = Id;
                result = (result * 397) ^ (Name?.GetHashCode() ?? 0);
                result = (result * 397) ^ (GroupName?.GetHashCode() ?? 0);
                result = (result * 397) ^ (ProjectGroupId ?? 0);
                result = (result * 397) ^ CreatedAt.GetHashCode();
                result = (result * 397) ^ UpdatedAt.GetHashCode();
                result = (result * 397) ^ (Minutes ?? 0);
                result = (result * 397) ^ (BillableMinutes ?? 0);
                result = (result * 397) ^ (UnbillableMinutes ?? 0);
                result = (result * 397) ^ (InvoicedMinutes ?? 0);
                result = (result * 397) ^ (RemainingMinutes ?? 0);
                result = (result * 397) ^ (BudgetMinutes ?? 0);
                result = (result * 397) ^ Billable.GetHashCode();
                result = (result * 397) ^ (ImportId ?? 0);
                result = (result * 397) ^ Enabled.GetHashCode();
                result = (result * 397) ^ (ColorHex?.GetHashCode() ?? 0);
                result = (result * 397) ^ Stepping;
                result = (result * 397) ^ (UserId ?? 0);
                result = (result * 397) ^ (Budget ?? 0);
                result = (result * 397) ^ (AccountId ?? 0);
                result = (result * 397) ^ (InvoiceRecipientDetails?.GetHashCode() ?? 0);
                result = (result * 397) ^ (CachedTags?.GetHashCode() ?? 0);
                return result;
            }
        }
    }
}