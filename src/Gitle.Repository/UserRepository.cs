namespace Gitle.Repository
{
    #region Usings

    using System.Linq;
    using Model;
    using Model.Interfaces.Repository;
    using NHibernate;

    #endregion

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ISession session;

        public UserRepository(ISessionFactory sessionFactory) : base(sessionFactory)
        {
            session = sessionFactory.GetCurrentSession();
        }

        public User FindByName(string name)
        {
            var users = session.QueryOver<User>().Where(x => x.IsActive).And(x => x.Name == name).List();
            return users.Count > 0 ? users.First() : new User.NullUser();
        }
    }
}