namespace Gitle.Model.Interfaces.Model
{
    using System;

    public interface IIssueAction
    {
        Issue Issue { get; set; } 
        User User { get; set; }
        DateTime CreatedAt { get; set; }
        string Text { get; }

    }
}