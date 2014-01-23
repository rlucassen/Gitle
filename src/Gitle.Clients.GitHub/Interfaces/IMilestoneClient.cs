namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IMilestoneClient
    {
        List<Milestone> List(string repo);
        Milestone Get(string repo, int milestoneId);
        Milestone Post(string repo, Milestone milestone);
    }
}