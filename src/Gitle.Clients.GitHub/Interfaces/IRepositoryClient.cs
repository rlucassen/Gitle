namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IRepositoryClient
    {
        List<Repository> List();
        Repository Get(string repo);
        IList<Hook> GetHooks(string repo);
        bool PostHook(string repo, string url);
    }
}