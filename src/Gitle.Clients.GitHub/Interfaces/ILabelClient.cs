namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface ILabelClient
    {
        List<Label> List(string repo);
        Label Get(string repo, string labelId);
        Label Post(string repo, Label label);
        Label Patch(string repo, string labelId, Label label);
        List<Label> AddLabelToIssue(string repo, int issueId, Label label);
    }
}