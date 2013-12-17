namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IMilestoneClient
    {
        List<Milestone> List(string fullrepo);
    }
}