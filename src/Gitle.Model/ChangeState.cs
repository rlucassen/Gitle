namespace Gitle.Model
{
    using System;
    using Enum;
    using Helpers;
    using Interfaces.Model;

    public class ChangeState : ModelBase, IIssueAction
    {
        public virtual IssueState IssueState { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual string Text { get { return string.Format("De taak is {0}{1}", IssueState.GetDescription(), User != null ? string.Format(" door {0}", User.FullName) : string.Empty); } }
    }
}