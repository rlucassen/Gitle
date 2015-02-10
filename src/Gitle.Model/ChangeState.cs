namespace Gitle.Model
{
    using System;
    using System.Linq;
    using Enum;
    using Helpers;
    using Interfaces.Model;

    public class ChangeState : ModelBase, IIssueAction
    {
        public virtual IssueState IssueState { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime CreatedAt { get; set; }

        public virtual string Text
        {
            get
            {
                var state = IssueState.GetDescription();
                var openings = Issue.ChangeStates.Where(x => x.IssueState == IssueState.Open).ToList();
                if (openings.Count() > 1 && openings.OrderByDescending(x => x.CreatedAt).Last() != this &&
                    IssueState == IssueState.Open)
                {
                    state = "heropend";
                }
                return string.Format("De taak is {0}{1}", state,
                                     User != null ? string.Format(" door {0}", User.FullName) : string.Empty);
            }
        }

        public virtual string HtmlText
        {
            get
            {
                var state = IssueState.GetDescription();
                var openings = Issue.ChangeStates.Where(x => x.IssueState == IssueState.Open).ToList();
                if (openings.Count() > 1 && openings.OrderByDescending(x => x.CreatedAt).Last() != this &&
                    IssueState == IssueState.Open)
                {
                    state = "heropend";
                }
                return string.Format("De taak is {0}{1} op <strong>{2}</strong>", state,
                                     User != null ? string.Format(" door <strong>{0}</strong>", User.FullName) : string.Empty, DateTimeHelper.Readable(CreatedAt));
            }
        }

        public virtual string EmailSubject
        {
            get {
                var state = IssueState.GetDescription();
                var openings = Issue.ChangeStates.Where(x => x.IssueState == IssueState.Open).ToList();
                if (openings.Count() > 1 && openings.OrderByDescending(x => x.CreatedAt).Last() != this &&
                    IssueState == IssueState.Open)
                {
                    state = "heropend";
                }
                return string.Format("Taak {2} is {0}{1}", state,
                                     User != null ? string.Format(" door {0}", User.FullName) : string.Empty, Issue.Number);
            }
        }
    }
}