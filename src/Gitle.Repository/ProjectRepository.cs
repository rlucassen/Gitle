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
            var users = Session.QueryOver<Project>().Where(x => x.IsActive).And(x => x.Slug == slug).List();
            return users.FirstOrDefault();
        }
    }
}