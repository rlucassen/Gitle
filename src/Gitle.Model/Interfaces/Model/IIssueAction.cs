namespace Gitle.Model.Interfaces.Model
{
    using System;

    public interface IIssueAction : ITouchable
    {
        Issue Issue { get; set; } 
        User User { get; set; }
        DateTime CreatedAt { get; set; }
        string Text { get; }
        string HtmlText { get; }
        string EmailSubject { get; }

        void Touch(User user);
        bool Touched(User user);
        bool TouchedBefore(User user, DateTime datetime);

    }
}