namespace Gitle.Repository
{
    using Model;
    using Model.Interfaces.Repository;
    using NHibernate;

    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository 
    {
        public CustomerRepository(ISessionFactory sessionFactory)
            : base(sessionFactory)
        {
        }
    }
}