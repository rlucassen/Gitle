namespace Gitle.Model
{
    using System;
    using Helpers;
    using Interfaces.Model;

    public class Pickup : Touchable, IIssueAction
    {
        public virtual Issue Issue { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime CreatedAt { get; set; }

        public virtual string Text
        {
            get
            {
                return
                    string.Format(
                        "De taak is opgepakt{0}",
                        User != null ? string.Format(" door <strong>{0}</strong>", User.FullName) : "");
            }
        }

        public virtual string HtmlText
        {
            get
            {
                return
                    string.Format(
                        "De taak is opgepakt{0} op <strong>{1}</strong>",
                        User != null ? string.Format(" door <strong>{0}</strong>", User.FullName) : "",
                        DateTimeHelper.Readable(CreatedAt));
            }
        }

        public virtual string EmailSubject { get { return string.Format("Taak {1} is opgepakt door {0}", User != null ? User.FullName : "", Issue.Number); } }

    }
}