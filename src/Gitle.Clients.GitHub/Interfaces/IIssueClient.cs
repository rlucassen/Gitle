namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IIssueClient
    {
        List<Issue> List(string repo, string state = "open,closed");
        List<Issue> List(string repo, int milestoneId, string state = "open,closed");
        Issue Get(string repo, int issueId);
        Issue Post(string repo, Issue issue);
        Issue Patch(string repo, int issueId, Issue issue);
    }
}