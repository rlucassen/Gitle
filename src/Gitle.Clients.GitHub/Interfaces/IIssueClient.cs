namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IIssueClient
    {
        List<Issue> List(string repo);
        List<Issue> List(string repo, int milestoneId);
        Issue Get(string repo, int issueId);
        Issue Post(string repo, Issue issue);
        Issue Patch(string repo, int issueId, Issue issue);
    }
}