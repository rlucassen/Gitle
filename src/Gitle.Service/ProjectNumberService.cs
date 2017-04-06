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

        public (int Initial, int Service, int Internal) GetNextProjectNumbers()
        {
            var year = DateTime.Today.Year;
            
            var projects = _session.Query<Project>().Where(x => x.Number.ToString().Length == 7);

            var initialProjects = projects.Where(x => x.Number > year*1000 && x.Number < year*1000+600);
            var newInitialProjectNumber = initialProjects.Any() ? initialProjects.Max(x => x.Number) + 1 : year * 1000 + 1;

            var serviceProjects = projects.Where(x => x.Number > year*1000+600 && x.Number < year*1000+900);
            var newServiceProjectNumber = serviceProjects.Any() ? serviceProjects.Max(x => x.Number) + 1 : year * 1000 + 601;

            var newInternalProjectNumber = year * 1000 + 900;

            return (newInitialProjectNumber, newServiceProjectNumber, newInternalProjectNumber);
        }
    }
}