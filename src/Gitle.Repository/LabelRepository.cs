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
        public LabelRepository(ISessionFactory sessionFactory)
            : base(sessionFactory)
        {
        }
    }
}