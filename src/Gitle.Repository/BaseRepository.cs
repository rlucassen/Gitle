namespace Gitle.Repository
{
    #region Usings

    using System.Collections.Generic;
    using Model;
    using Model.Enum;
    using Model.Interfaces.Repository;
    using NHibernate;

    #endregion

    public abstract class BaseRepository<T> : IBaseRepository<T> where T : ModelBase
    {
        protected ISession Session;

        public BaseRepository(ISessionFactory sessionFactory)
        {
            Session = sessionFactory.GetCurrentSession();
        }

        public T Get(long id)
        {
            return Session.Get<T>(id);
        }

        public IList<T> FindAll(FindOption findOption = FindOption.Active)
        {
            var queryOver = Session.QueryOver<T>();

            if (findOption != FindOption.Both)
                queryOver.Where(i => i.IsActive == (findOption == FindOption.Active));

            return queryOver.List();
        }

        public bool Save(T item)
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.SaveOrUpdate(item);
                tx.Commit();
            }
            
            return true;
        }

        public void Delete(T item)
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.Delete(item);
                tx.Commit();
            }
        }
    }
}