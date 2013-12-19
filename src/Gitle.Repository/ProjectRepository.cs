namespace Gitle.Repository
{
    using System.Collections.Generic;
    using System.Linq;
    using Clients.GitHub.Interfaces;
    using FluentNHibernate.Utils;
    using Model;
    using Model.Enum;
    using Model.Interfaces.Repository;
    using NHibernate;

    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        public ProjectRepository(ISessionFactory sessionFactory)
            : base(sessionFactory)
        {
        }

        public Project FindBySlug(string slug)
        {
            var projects = Session.QueryOver<Project>().Where(x => x.IsActive).And(x => x.Slug == slug).List();
            return projects.FirstOrDefault();
        }

        public IList<Project> FindByRepoAndMilestone(string repo, int milestoneId)
        {
            var projects = Session.QueryOver<Project>().Where(x => x.IsActive).And(x => x.Repository == repo).And(x => x.MilestoneId == milestoneId).List();
            return projects.ToList();
        }
    }
}