namespace Gitle.Service
{
    using System;
    using System.Linq;
    using Model;
    using Model.Interfaces.Service;
    using NHibernate;
    using NHibernate.Linq;

    public class ProjectNumberService : IProjectNumberService
    {
        private readonly ISession _session;

        public ProjectNumberService(ISessionFactory sessionFactory)
        {
            _session = sessionFactory.GetCurrentSession();
        }

        public int GetNextProjectNumber()
        {
            var maxNumber = _session.Query<Project>().Max(x => x.Number);

            var year = maxNumber.ToString().Substring(0, 4);

            if (year != DateTime.Today.Year.ToString())
            {
                return int.Parse($"{DateTime.Today.Year}001");
            }

            return maxNumber + 1;
        }
    }
}