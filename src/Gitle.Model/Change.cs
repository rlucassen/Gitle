namespace Gitle.Model
{
    using System;
    using Interfaces.Model;

    public class Change : ModelBase, IIssueAction
    {
        public virtual Issue Issue { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual string Text { get { return string.Format("De taak is aangepast door {0}", User != null ? User.FullName : string.Empty); } }
        public virtual string EmailSubject { get { return string.Format("Taak {1} is aangepast door {0}", User != null ? User.FullName : string.Empty, Issue.Number); } }
    }
}