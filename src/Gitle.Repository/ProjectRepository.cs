namespace Gitle.Repository
{
    using Model;
    using Model.Interfaces.Repository;
    using NHibernate;

    public class ProjectRepository : BaseRepository<Project>, IProjectRepository 
    {
        public ProjectRepository(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }
    }
}