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

    public class LabelRepository : BaseRepository<Label>, ILabelRepository
    {
        private readonly ISession session;
        public LabelRepository(ISessionFactory sessionFactory)
            : base(sessionFactory)
        {
            session = sessionFactory.GetCurrentSession();
        }

        public Label FindByName(string name)
        {
            var labels = session.QueryOver<Label>().Where(x => x.IsActive).And(x => x.Name == name).List();
            return labels.FirstOrDefault();
        }
    }
}