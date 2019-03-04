namespace Gitle.Model
{
    using System;
    using System.Linq;
    using Enum;
    using Helpers;
    using Interfaces.Model;
    using Localization;

    public class ChangeState : Touchable, IIssueAction
    {
        public virtual IssueState IssueState { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime CreatedAt { get; set; }

        public virtual string Text
        {
            get
            {
                var state = Language.ResourceManager.GetString(IssueState.ToString()).ToLower();
                var openings = Issue.ChangeStates.Where(x => x.IssueState == IssueState.Open).ToList();
                if (openings.Count() > 1 && openings.OrderByDescending(x => x.CreatedAt).Last() != this &&
                    IssueState == IssueState.Open)
                {
                    state = "heropend";
                }
                return string.Format("De taak is {0}{1}", state,
                                     User != null ? string.Format(" door {0}", User.FullName) : "");
            }
        }

        public virtual string HtmlText
        {
            get
            {
                var state = Language.ResourceManager.GetString(IssueState.ToString()).ToLower();
                var openings = Issue.ChangeStates.Where(x => x.IssueState == IssueState.Open).ToList();
                if (openings.Count() > 1 && openings.OrderByDescending(x => x.CreatedAt).Last() != this &&
                    IssueState == IssueState.Open)
                {
                    state = "heropend";
                }
                return string.Format("De taak is {0}{1} op <strong>{2}</strong>", state,
                                     User != null ? string.Format(" door <strong>{0}</strong>", User.FullName) : "", DateTimeHelper.Readable(CreatedAt));
            }
        }

        public virtual string EmailSubject
        {
            get {
                var state = Language.ResourceManager.GetString(IssueState.ToString()).ToLower();
                var openings = Issue.ChangeStates.Where(x => x.IssueState == IssueState.Open).ToList();
                if (openings.Count() > 1 && openings.OrderByDescending(x => x.CreatedAt).Last() != this &&
                    IssueState == IssueState.Open)
                {
                    state = "heropend";
                }
                return string.Format("Taak {2} is {0}{1}", state,
                                     User != null ? string.Format(" door {0}", User.FullName) : "", Issue.Number);
            }
        }
    }
}