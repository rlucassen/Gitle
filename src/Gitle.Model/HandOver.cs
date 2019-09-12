namespace Gitle.Model
{
    using System;
    using Helpers;
    using Interfaces.Model;

    public class HandOver : Touchable, IIssueAction
    {
        public virtual Issue Issue { get; set; }
        public virtual User User { get; set; }
        public virtual User ByUser { get; set; }
        public virtual DateTime CreatedAt { get; set; }


        public virtual string HandedOverByText
        {
            get
            {
                return
                    string.Format(
                        "De taak is doorgegeven{0} op <strong>{1}</strong>",
                        ByUser != null ? string.Format(" door <strong>{0}</strong>", ByUser.FullName) : "",
                        DateTimeHelper.Readable(CreatedAt));
            }
        }

        public virtual string Text
        {
            get
            {
                return
                    string.Format(
                        "De taak is doorgegeven{0}{1}",
                        User != null ? string.Format(" aan <strong>{0}</strong>", User.FullName) : "",
                        ByUser != null ? string.Format(" door <strong>{0}</strong>", ByUser.FullName) : "");
            }
        }

        public virtual string HtmlText
        {
            get
            {
                return
                    string.Format(
                        "De taak is doorgegeven{0}{1} op <strong>{2}</strong>",
                        User != null ? string.Format(" aan <strong>{0}</strong>", User.FullName) : "",
                        ByUser != null ? string.Format(" door <strong>{0}</strong>", ByUser.FullName) : "",
                        DateTimeHelper.Readable(CreatedAt));
            }
        }

        public virtual string EmailSubject { get { return string.Format("Taak {2} is doorgegeven{0}{1}",
                                                                        User != null ? string.Format(" aan {0}", User.FullName) : "",
                                                                        ByUser != null ? string.Format(" door {0}", ByUser.FullName) : "",
                                                                        Issue.Number); } }


    }
}