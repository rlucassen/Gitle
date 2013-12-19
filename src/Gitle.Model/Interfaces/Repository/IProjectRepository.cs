namespace Gitle.Model.Interfaces.Repository
{
    using System.Collections.Generic;
    using Enum;

    public interface IProjectRepository : IBaseRepository<Project>
    {
        Project FindBySlug(string slug);
        IList<Project> FindByRepoAndMilestone(string repo, int milestoneId);
    }
}