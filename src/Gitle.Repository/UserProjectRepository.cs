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

    public class UserProjectRepository : BaseRepository<UserProject>, IUserProjectRepository
    {
        public UserProjectRepository(ISessionFactory sessionFactory)
            : base(sessionFactory)
        {
        }

    }
}