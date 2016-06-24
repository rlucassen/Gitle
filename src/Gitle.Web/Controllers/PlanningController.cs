namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Model;
    using NHibernate;
    using NHibernate.Linq;

    public class PlanningController : SecureController
    {
        public PlanningController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void Index()
        {
            
        }

        [return: JSONReturnBinder]
        public List<Event> Events(DateTime start, DateTime end)
        {
            return session.Query<PlanningItem>()
                .Where(x => x.End > start && x.Start < end)
                .Select(x => new Event
                {
                    id = x.Id.ToString(),
                    resourceId = x.Project.Slug,
                    start = x.Start.ToString("K"),
                    end = x.End.ToString("K"),
                    title = x.User.FullName,
                    color = x.User.Color
                }).ToList();
        }

        [return: JSONReturnBinder]
        public List<Resource> Projects(DateTime start, DateTime end)
        {
            return session.Query<PlanningItem>()
                .Where(x => x.End > start && x.Start < end)
                .Select(x => x.Project).Distinct().Select(x => new Resource
                {
                    id = x.Slug,
                    title = x.Name
                }).ToList();
        }
    }

    public class Event
    {
        public string id;
        public string resourceId;
        public string start;
        public string end;
        public string title;
        public string color;
        public string borderColor = "#666";
    }

    public class Resource
    {
        public string id;
        public string title;
    }
}