namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Routing;
    using Clients.Freckle.Interfaces;
    using Clients.Freckle.Models;
    using FluentNHibernate.Conventions.Inspections;
    using Helpers;
    using Model;
    using Model.Enum;
    using Model.Helpers;
    using NHibernate;
    using NHibernate.Linq;
    using Newtonsoft.Json;
    using QueryParsers;
    using Issue = Model.Issue;
    using Project = Model.Project;

    public class IssueController : SecureController
    {
        private readonly ISessionFactory sessionFactory;
        private readonly IEntryClient entryClient;

        public IssueController(ISessionFactory sessionFactory, IEntryClient entryClient) : base(sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            this.entryClient = entryClient;
        }

        [MustHaveProject]
        public void Index(string projectSlug, string query)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);

            var parser = new IssueQueryParser(sessionFactory, query, project, CurrentUser);

            var filterPresets = session.Query<FilterPreset>().Where(x => x.User == CurrentUser && x.IsActive && (x.Project == null || (x.Project != null && x.Project.Id == project.Id))).ToList();
            var globalFilterPresets = session.Query<FilterPreset>().Where(x => x.User == null && x.IsActive && (x.Project == null || (x.Project != null && x.Project.Id == project.Id))).ToList();

            PropertyBag.Add("project", project);
            PropertyBag.Add("result", parser);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.VisibleForCustomer).ToList());
            PropertyBag.Add("customerLabels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer).ToList());
            PropertyBag.Add("filterPresets", filterPresets);
            PropertyBag.Add("globalFilterPresets", globalFilterPresets);
            PropertyBag.Add("allAdmins", session.Query<User>().Where(x => x.IsAdmin).ToList());
            PropertyBag.Add("selectedPickupbys", parser.SelectedPickupbys);
            PropertyBag.Add("pickupany", parser.PickupAny);
            PropertyBag.Add("pickupnone", parser.PickupNone);
            PropertyBag.Add("dump", CreateDummyIssue(project));
        }

        public DummyIssue CreateDummyIssue(Project project)
        {
            var startDate = DateTime.Parse("2016-06-01");
            var dumpBookings = session.Query<Booking>().Where(b => b.Project == project && b.Issue == null && b.Date >= startDate);
            DummyIssue dummy = new DummyIssue {Name = "DUMP: " + project.Name, Bookings = dumpBookings.ToList()};
            return dummy;
        }

        [MustHaveProject]
        public void View(string projectSlug, int issueId)
        {
            if (!string.IsNullOrEmpty(Request.UrlReferrer))
            {
                var referer = new Uri(Request.UrlReferrer);
                var routeMatch = RoutingModuleEx.Engine.FindMatch(referer.AbsolutePath, new RouteContext(Request, null, "/", new Hashtable()));
                if (routeMatch.Name == "issues") PropertyBag.Add("referer", referer.PathAndQuery);
            }

            var project = session.Slug<Project>(projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            PropertyBag.Add("item", item);
            PropertyBag.Add("comments", item.Comments);
            PropertyBag.Add("days", DayHelper.GetPastDaysList());
            PropertyBag.Add("datetime", DateTime.Now);

            item.Touch(CurrentUser);
            foreach(var action in item.Actions)
            {
                action.Touch(CurrentUser);
            }
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                transaction.Commit();
            }
        }

        [MustHaveProject]
        public void QuickView(string projectSlug, int issueId)
        {
            var project = session.Slug<Project>(projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            PropertyBag.Add("item", item);
            PropertyBag.Add("comments", item.Comments);
            PropertyBag.Add("datetime", DateTime.Now);
            CancelLayout();
        }

        [MustHaveProject]
        public void BookingsChart(string projectSlug, int issueId)
        {
            var project = session.Slug<Project>(projectSlug);
            var item = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            var totalTime = item.Bookings.Sum(x => x.Hours);
            var bookings = item.Bookings.GroupBy(x => x.User).ToDictionary(x => x.Key, x => new { percentage = Math.Round(x.Sum(y => y.Hours) / totalTime * 100), percentageBillable = Math.Round(x.Where(y => !y.Unbillable).Sum(y => y.Hours) / totalTime * 100), total = x.Sum(y => y.Hours), totalBillable = x.Where(y => !y.Unbillable).Sum(y => y.Hours) });
            PropertyBag.Add("project", project);
            PropertyBag.Add("item", item);
            PropertyBag.Add("bookings", bookings);
            PropertyBag.Add("totalBooked", item.BillableBookingHoursString());
            PropertyBag.Add("totalBookedUnbillable", item.UnbillableBookingHoursString());
            CancelLayout();
        }

        [MustHaveProject]
        public void New(string projectSlug)
        {
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            PropertyBag.Add("project", project);
            PropertyBag.Add("item", new Issue(){Project = project});
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug, int issueId)
        {
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            PropertyBag.Add("item", item);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
        }

        [MustHaveProject]
        public void Save(string projectSlug, int issueId, string[] labels, string andNew)
        {
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }

            var issue = session.Query<Issue>().SingleOrDefault(i => i.Number == issueId && i.Project == project);

            var savedIssue = SaveIssue(project, issue, labels);

            var hash = $"#issue{savedIssue.Number}";
            if (string.IsNullOrEmpty(andNew))
            {
                RedirectToUrl($"/project/{project.Slug}/issue/index{hash}");
            }
            else
            {
                RedirectToUrl($"/project/{project.Slug}/issue/new");
            }
        }

        [return: JSONReturnBinder]
        public object AjaxSave(string projectSlug, int issueId, string[] labels)
        {
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }

            var issue = session.Query<Issue>().SingleOrDefault(i => i.Number == issueId && i.Project == project);

            var savedIssue = SaveIssue(project, issue, labels);

            return new {savedIssue.Id, savedIssue.Number, savedIssue.Name};
        }

        private Issue SaveIssue(Project project, Issue issue, string[] labels)
        {
            if (issue != null)
            {
                BindObjectInstance(issue, "item");
                issue.Change(CurrentUser);
            }
            else
            {
                issue = BindObject<Issue>("item");
                issue.Number = project.NewIssueNumber;
                issue.Project = project;
                issue.Open(CurrentUser);
            }

            issue.Labels = labels.Select(label => session.Query<Label>().FirstOrDefault(l => l.Name == label)).ToList();

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(issue);
                transaction.Commit();
            }
            return issue;
        }


        [Admin]
        public void Pickup(string projectSlug, int issueId)
        {
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            var issue = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            if (!issue.IsArchived && issue.PickedUpBy != CurrentUser)
            {
                issue.Pickup(CurrentUser);
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(issue);
                    transaction.Commit();
                }
            }
            RedirectToReferrer();
        }

        [MustHaveProject]
        public void AddComment(string projectSlug, int issueId, string body)
        {
            RedirectToReferrer();
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            var issue = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            if (issue.IsArchived) return;
            var comment = new Comment
                              {
                                  Text = body,
                                  CreatedAt = DateTime.Now,
                                  Issue = issue,
                                  User = CurrentUser
                              };
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(comment);
                transaction.Commit();
            }
        }

        [MustHaveProject]
        public void AddLabel(string projectSlug, int issueId, int param)
        {
            RedirectToReferrer();
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            var issue = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            if (issue.IsArchived) return;
            var label = project.Labels.First(l => l.Id == param);
            issue.Labels.Add(label);
            issue.Change(CurrentUser);

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(issue);
                transaction.Commit();
            }
        }

        [Admin]
        public void BookTime(string projectSlug, int issueId, DateTime date, double minutes, bool close, string comment)
        {
            RedirectToReferrer();
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            var issue = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            if (issue.IsArchived) return;
            var booking = new Booking {User = CurrentUser, Date = date, Minutes = minutes, Issue = issue, Project = project, Comment = comment};

            using (var tx = session.BeginTransaction())
            {
                if (close)
                {
                    issue.Close(CurrentUser);
                    session.SaveOrUpdate(issue);
                }
                session.SaveOrUpdate(booking);
                tx.Commit();
            }
        }

        [MustHaveProject]
        public void Close(string projectSlug, int issueId)
        {
            RedirectToReferrer();
            var project = session.Slug<Project>(projectSlug);
            var issue = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            if (issue.State == IssueState.Open || issue.State == IssueState.Unknown)
            {
                issue.Close(CurrentUser);
                using (var tx = session.BeginTransaction())
                {
                    session.SaveOrUpdate(issue);
                    tx.Commit();
                }
            }
        }

        [MustHaveProject]
        public void Reopen(string projectSlug, int issueId)
        {
            RedirectToReferrer();
            var project = session.Slug<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            var issue = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            if (issue.State == IssueState.Closed || issue.State == IssueState.Unknown)
            {
                issue.Open(CurrentUser);
                using (var tx = session.BeginTransaction())
                {
                    session.SaveOrUpdate(issue);
                    tx.Commit();
                }
            }
        }

        [Admin]
        public void Archive(string projectSlug, int issueId)
        {
            RedirectToReferrer();
            if (!CurrentUser.IsAdmin) return;
            var project = session.Slug<Project>(projectSlug);
            var issue = session.Query<Issue>().Single(i => i.Number == issueId && i.Project == project);
            issue.Archive(CurrentUser);
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(issue);
                tx.Commit();
            }
        }

        [MustHaveProject]
        public void ReOrderIssue(string projectSlug, int issueNumber, int newIndex)
        {
            var issues = session.Query<Issue>().Where(x => x.Project.Slug == projectSlug && x.Pickups.Count == 0).ToList().Where(x => x.ChangeStates.Last().IssueState == IssueState.Open).OrderBy(x => x.PrioOrder).ToList();
            
            var issue = session.Query<Issue>().Single(x => x.Project.Slug == projectSlug && x.Number == issueNumber);
            issues.Remove(issue);
            issue.Prioritized = true;
            issues.Insert(newIndex, issue);

            var order = 1;
            using (var tx = session.BeginTransaction())
            {
                foreach (var currentissue in issues)
                {
                    currentissue.PrioOrder = order;
                    order++;
                    session.SaveOrUpdate(currentissue);
                }
                tx.Commit();
            }

            CancelView();
        }

        [return: JSONReturnBinder]
        public Dictionary<int, int> GetPrioOrder(string projectSlug)
        {
            var issues = session.Query<Issue>().Where(x => x.Project.Slug == projectSlug && x.Pickups.Count == 0).ToList().Where(x => x.ChangeStates.Last().IssueState == IssueState.Open).OrderByDescending(x => x.Prioritized).ThenBy(x => x.PrioOrder).ThenByDescending(x => x.Number);
            return issues.Select((x, i) => new {x.Number, i}).ToDictionary(x => x.Number, x => x.i);
        }

        [MustHaveProject]
        public void ExportImport(string projectSlug)
        {
            var project = session.Slug<Project>(projectSlug);

            PropertyBag.Add("project", project);
        }

        [MustHaveProject]
        public void ExportJson(string projectSlug, string[] selectedLabels, IssueState state, string issues)
        {
            var project = session.Slug<Project>(projectSlug);

            if (Request.Params["state"] == null) state = CurrentUser.DefaultState;
            var items = project.Issues;

            if (string.IsNullOrEmpty(issues))
            {
                if (selectedLabels.Length > 0)
                    items = items.Where(
                            i => i.Labels.Select(l => l.Name).Intersect(selectedLabels).Count() == selectedLabels.Length).ToList();
            }
            else
            {
                items = items.Where(i => issues.Split(',').Contains(i.Number.ToString())).ToList();
            }

            var json = JsonConvert.SerializeObject(items, new JsonSerializerSettings{ DefaultValueHandling = DefaultValueHandling.Ignore, DateParseHandling = DateParseHandling.None});

            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = $"issues_{project.Slug}_{DateTime.Now:yyyyMMdd_hhmm}.json";

            Response.AppendHeader("content-disposition", $"attachment; filename={filename}");
            Response.ContentType = "application/json";

            var byteArray = Encoding.Default.GetBytes(json);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }

        [MustHaveProject]
        public void ExportCsv(string projectSlug, string[] selectedLabels, IssueState state, string issues)
        {
            var project = session.Slug<Project>(projectSlug);

            if (Request.Params["state"] == null) state = CurrentUser.DefaultState;
            var items = project.Issues;

            if (string.IsNullOrEmpty(issues))
            {
                if (selectedLabels.Length > 0)
                    items = items.Where(
                            i => i.Labels.Select(l => l.Name).Intersect(selectedLabels).Count() == selectedLabels.Length).ToList();
            }
            else
            {
                items = items.Where(i => issues.Split(',').Contains(i.Number.ToString())).ToList();
            }

            var csv = CsvHelper.IssuesCsv(project, items);
            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = $"issues_{project.Slug}_{DateTime.Now:yyyyMMdd_hhmm}.csv";

            Response.AppendHeader("content-disposition", $"attachment; filename={filename}");
            Response.ContentType = "application/csv";

            var byteArray = Encoding.Default.GetBytes(csv);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }

        [return: JSONReturnBinder]
        public object Autocomplete(long projectId, string query)
        {
            var suggestions = new List<Suggestion>();
            var issues = session.Query<Issue>().Where(i => i.Project.Id == projectId);
            if (query != null)
            {
                issues = issues.Where(i => i.Number.ToString().Contains(query) || i.Name.Contains(query));
            }
            suggestions.AddRange(issues.ToList().Where(i => i.HasBeenOpenSince(DateTime.Today.AddDays(-7))).Select(x => new Suggestion($"#{x.Number} - {x.Name}", x.Id.ToString())));
            return new { query = query, suggestions = suggestions };
        }

        [return: JSONReturnBinder]
        public object Suggestions(long projectId, string query)
        {
            var suggestions = new List<Suggestion>();

            query = query?.ToLower() ?? "";

            var issues = session.Query<Issue>().Where(i => i.Project.Id == projectId).ToList().Where(i => i.HasBeenOpenSince(DateTime.Today.AddDays(-7))).ToList();

            if (!string.IsNullOrEmpty(query))
            {
                issues = issues.Where(x => x.Number.ToString().StartsWith(query) || x.Name.ToLower().Contains(query) || x.Comments.Any(c => c.Id.ToString().StartsWith(query))).ToList();
            }

            foreach (var issue in issues)
            {
                suggestions.Add(new Suggestion($"#{issue.Number} - {issue.Name}", issue.Number.ToString(), "issuelink"));
                var comments = issue.Comments;
                suggestions.AddRange(comments.Select(c => new Suggestion($"#{issue.Number}#{c.Id} - {c.Name} ({c.CreatedAt})", $"{issue.Number}#{c.Id}", "commentlink")));
            }
            return suggestions;
        }
    }
}