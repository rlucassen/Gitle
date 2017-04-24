namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Castle.MonoRail.Framework;
    using Helpers;
    using Model;
    using Model.Helpers;
    using NHibernate;
    using NHibernate.Criterion;
    using NHibernate.Linq;

    // Colors
        //008CBA
        //F04124
        //43AC6A
        //FFBF00
        //9FFFCB
        //FFFFFF
        //F966FF
        //B96ECC
        //E83F6F

    public class PlanningController : SecureController
    {
        public PlanningController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void Index()
        {
            var employees = session.Query<User>().Where(x => x.IsActive && x.IsAdmin && x.Color != null);
            PropertyBag.Add("employees", employees);
            PropertyBag.Add("comments", session.Query<PlanningComment>().Where(x => x.IsActive));
        }

        [return: JSONReturnBinder]
        public List<Event> Events(DateTime start, DateTime end)
        {
            return session.Query<PlanningItem>()
                .Where(x => x.End > start && x.Start < end && x.IsActive).ToList()
                .Select(x => new Event
                {
                    id = x.Id,
                    resourceId = x.Resource,
                    start = x.Start.ToString("yyyy-MM-dd HH:mm"),
                    end = x.End.ToString("yyyy-MM-dd HH:mm"),
                    title = x.User.FullName,
                    color = $"#{x.User.Color}",
                    userId = x.User.Id.ToString(),
                }).ToList();
        }

        [return: JSONReturnBinder]
        public List<Resource> Projects(DateTime start, DateTime end)
        {
            //var year = start.Year;
            //var week = start.WeekNr();

            IList<(int year, int startWeek, int endWeek)> list = new List<(int, int, int)>();

            for (int yearIndex = start.Year; yearIndex <= end.Year; yearIndex++)
            {
                var startWeek = yearIndex == start.Year ? start.WeekNr() : 1;
                var endWeek = yearIndex == end.Year ? end.WeekNr() : 53;
                list.Add((yearIndex, startWeek, endWeek));
            }

            var disjunction = Restrictions.Disjunction();

            foreach (var tuple in list)
            {
                disjunction.Add<PlanningResource>(x => x.Year == tuple.year && x.Week >= tuple.startWeek && x.Week <= tuple.endWeek);
            }

            return session.QueryOver<PlanningResource>().Where(disjunction).List()
                //.Where(x => x.Year >= year && x.Week == week).List()
                .Select(x => new Resource(x)).ToList();
        }

        [return: JSONReturnBinder]
        public Resource SaveResource(int year, int week, long projectId, long[] issueIds)
        {
            var planningResource = session.Query<PlanningResource>().FirstOrDefault(x => x.Project.Id == projectId && x.Year == year && x.Week == week);

            if (planningResource == null)
            {
                planningResource = new PlanningResource
                {
                    Year = year,
                    Week = week,
                    Project = session.Get<Project>(projectId),
                    Issues = session.Query<Issue>().Where(x => issueIds.Contains(x.Id)).ToList()
                };
            }
            else
            {
                foreach (var issueId in issueIds)
                {
                    if(!planningResource.Issues.Select(x => x.Id).Contains(issueId))
                        planningResource.Issues.Add(session.Get<Issue>(issueId));
                }
            }


            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(planningResource);
                tx.Commit();
            }

            return new Resource(planningResource);
        }

        public void DeleteResource(long id, long issueId)
        {
            var planningResource = session.Get<PlanningResource>(id);

            using (var tx = session.BeginTransaction())
            {
                if (issueId > 0)
                {
                    planningResource.Issues.Remove(planningResource.Issues.FirstOrDefault(x => x.Id == issueId));
                    session.SaveOrUpdate(planningResource);
                }
                else
                {
                    session.Delete(planningResource);
                }

                tx.Commit();
            }
            RenderText("ok");
        }

        public void UpdateEvent(long eventId, long userId, string resource, DateTime start, DateTime end)
        {
            var planningItem = session.Get<PlanningItem>(eventId);

            if (planningItem == null)
            {
                planningItem = new PlanningItem()
                {
                    User = session.Get<User>(userId),
                    Resource = resource,
                    Start = start,
                    End = end
                };
            }
            else
            {
                planningItem.User = session.Get<User>(userId);
                planningItem.Resource = resource;
                planningItem.Start = start;
                planningItem.End = end;
            }

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(planningItem);
                tx.Commit();
            }

            RenderText(planningItem.Id.ToString());
        }

        public void DeleteEvent(long eventId)
        {
            var planningItem = session.Get<PlanningItem>(eventId);
            planningItem.Deactivate();

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(planningItem);
                tx.Commit();
            }

            RenderText("ok");
        }

        [Admin]
        public void Comment(string slug, string comment)
        {
            var item = session.SlugOrDefault<PlanningComment>(slug);

            item.Comment = comment;
            item.User = CurrentUser;

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RenderText(comment);
        }
    }

    public class Event
    {
        public long id;
        public string resourceId;
        public string userId;
        public string start;
        public string end;
        public string title;
        public string color;
        public string borderColor = "#666";
    }

    public class Resource
    {
        public Resource()
        {
        }

        public Resource(PlanningResource resource)
        {
            originalId = resource.Id;
            id = $"p{resource.Project.Id}";
            title = resource.Project.Name;
            children = resource.Issues.Select(i => new Resource
            {
                id = $"i{i.Id}",
                title = $"#{i.Number} - {i.Name}",
                originalId = i.Id
            }).ToList();
        }

        public long originalId;
        public string id;
        public string title;
        public IList<Resource> children = new List<Resource>();
    }
}