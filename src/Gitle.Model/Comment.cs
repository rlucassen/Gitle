namespace Gitle.Model
{
    using System;
    using Interfaces.Model;

    public class Comment : ModelBase, IIssueAction
    {
        public virtual string Text { get; set; }
        public virtual DateTime CreatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Issue Issue { get; set; }

        public virtual string Name { get { return User != null ? User.FullName : "Auxilium"; } }

        public virtual string EmailSubject { get { return string.Format("Er is gereageerd op taak {1} door {0}", User != null ? User.FullName : string.Empty, Issue.Number); } }
    }
}