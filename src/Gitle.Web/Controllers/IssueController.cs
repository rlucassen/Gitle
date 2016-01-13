namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Routing;
    using Clients.Freckle.Interfaces;
    using Clients.Freckle.Models;
    using Helpers;
    using Model;
    using Model.Enum;
    using Model.Helpers;
    using Model.Interfaces.Model;
    using NHibernate;
    using NHibernate.Linq;
    using Newtonsoft.Json;
    using Issue = Model.Issue;
    using Project = Model.Project;

    public class IssueController : SecureController
    {
        private readonly ISessionFactory sessionFactory;
        private readonly ISession session;
        private readonly IEntryClient entryClient;

        public IssueController(ISessionFactory sessionFactory, IEntryClient entryClient)
        {
            this.sessionFactory = sessionFactory;
            this.session = sessionFactory.GetCurrentSession();
            this.entryClient = entryClient;
        }

        [MustHaveProject]
        public void Index(string projectSlug, string query)
        {
            query = query ?? string.Empty;

            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

            var matches = Regex.Matches(query, @"[a-zA-Z0-9_]+:(([a-zA-Z0-9_,.]+)|('[a-zA-Z0-9_,. ]+'))");

            var allSorts = new Dictionary<string, string>()
                            {
                                {"CreatedBy", "Aanmelder"}, 
                                {"CreatedAt", "Aanmaakdatum"},
                                {"PickedUpBy", "Behandelaar"},
                                {"PickedUpAt", "Behandeldatum"},
                                {"Number", "Nummer"},
                                {"Name", "Naam"},
                                {"TotalHours", "Inspanning"},
                                {"Comments.Count", "Aantal reacties"}
                            };

            IList<string> selectedLabels = new List<string>();
            IList<string> notSelectedLabels = new List<string>();
            IList<IssueState> states = new List<IssueState>();
            IList<long> ids = new List<long>();
            IList<string> involveds = new List<string>();
            IList<string> openedbys = new List<string>();
            IList<string> closedbys = new List<string>();
            IList<string> pickupbys = new List<string>();
            IList<User> selectedPickupbys = new List<User>();
            IDictionary<string, bool> querySorts = new Dictionary<string, bool>();
            IDictionary<string, bool> linqSorts = new Dictionary<string, bool>();
            IDictionary<string, bool> selectedSorts = new Dictionary<string, bool>();
            var searchQuery = query;
            bool pickupany = false;
            bool pickupnone = false;

            foreach (Match match in matches)
            {
                var parts = match.Value.Split(':');
                var value = parts[1].Replace("'", "");
                searchQuery = searchQuery.Replace(match.Value, "");
                switch (parts[0])
                {
                    case "label":
                        selectedLabels.Add(value);
                        break;
                    case "notlabel":
                        notSelectedLabels.Add(value);
                        break;
                    case "state":
                        states.Add((IssueState)Enum.Parse(typeof(IssueState), value));
                        break;
                    case "id":
                        ids = value.Split(',').Select(long.Parse).ToList();
                        break;
                    case "involved":
                        involveds.Add(value == "me" ? CurrentUser.Name : value);
                        break;
                    case "opened":
                        openedbys.Add(value == "me" ? CurrentUser.Name : value);
                        break;
                    case "closed":
                        closedbys.Add(value == "me" ? CurrentUser.Name : value);
                        break;
                    case "pickup":
                        if (value == "any")
                        {
                            pickupany = true;
                            break;
                        }
                        if (value == "none")
                        {
                            pickupnone = true;
                            break;
                        }
                        pickupbys.Add(value == "me" ? CurrentUser.Name : value);
                        break;
                    case "sort":
                        selectedSorts.Add(value, false);
                        if (NHibernateMetadataHelper.IsMapped<Issue>(sessionFactory, value))
                            querySorts.Add(value, false);
                        else
                            linqSorts.Add(value, false);
                        break;
                    case "sortdesc":
                        selectedSorts.Add(value, true);
                        if (NHibernateMetadataHelper.IsMapped<Issue>(sessionFactory, value))
                            querySorts.Add(value, true);
                        else
                            linqSorts.Add(value, true);
                        break;
                }
            }


            var itemsQuery =
                session.Query<Issue>().Where(
                    x =>
                    x.Project == project &&
                    x.Labels.Count(l => selectedLabels.Contains(l.Name)) == selectedLabels.Count &&
                    !x.Labels.Any(l => notSelectedLabels.Contains(l.Name)));

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchQueryParts = searchQuery.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
                foreach (var searchQueryPart in searchQueryParts)
                {
                    var trimmedSearchQueryPart = searchQueryPart.Trim();
                    itemsQuery = itemsQuery.Where(x => x.Name.Contains(trimmedSearchQueryPart) || x.Body.Contains(trimmedSearchQueryPart));
                }
            }

            if (ids.Any())
                itemsQuery = itemsQuery.Where(x => ids.Contains(x.Number));

            foreach (var involved in involveds)
            {
                itemsQuery =
                    itemsQuery.Where(
                        x =>
                        x.Comments.Any(a => a.User != null && (a.User.Name == involved || a.User.FullName == involved)) ||
                        x.ChangeStates.Any(a => a.User != null && (a.User.Name == involved || a.User.FullName == involved)) ||
                        x.Changes.Any(a => a.User != null && (a.User.Name == involved || a.User.FullName == involved)));
            }

            foreach (var openedby in openedbys)
            {
                itemsQuery = itemsQuery.Where(
                    x => x.ChangeStates.Any(
                        a => a.IssueState == IssueState.Open && a.User != null &&
                             (a.User.Name == openedby || a.User.FullName == openedby)));
            }

            foreach (var closedby in closedbys)
            {
                itemsQuery = itemsQuery.Where(
                    x => x.ChangeStates.Any(
                        a => a.IssueState == IssueState.Closed && a.User != null &&
                             (a.User.Name == closedby || a.User.FullName == closedby)));
            }

            foreach (var pickupby in pickupbys)
            {
                selectedPickupbys.Add(session.Query<User>().FirstOrDefault(x => x.Name == pickupby || x.FullName == pickupby));
            }

            if (pickupany)
            {
                itemsQuery = itemsQuery.Where(x => x.Pickups.Any());
            }

            if (pickupnone)
            {
                itemsQuery = itemsQuery.Where(x => x.Pickups.Count == 0);
            }

            if (pickupbys.Any())
            {
                itemsQuery = itemsQuery.Where(
                    x => x.Pickups.Any(
                        a => a.User != null && pickupbys.Contains(a.User.Name) || pickupbys.Contains(a.User.FullName)));
            }

            if (querySorts.Count > 0)
            {
                foreach (var sort in querySorts)
                {
                    itemsQuery = itemsQuery.OrderByProperty(sort.Key, sort.Value);
                }
            }

            var items = itemsQuery.ToList();

            if (linqSorts.Count > 0)
            {
                foreach (var sort in linqSorts)
                {
                    items = items.OrderByProperty(sort.Key, sort.Value).ToList();
                }
            }

            if(querySorts.Count == 0 && linqSorts.Count == 0) 
            {
                items = items.OrderBy(x => x.State).ThenBy(x => x.Pickups.Count == 0).ThenBy(x => x.PrioOrder == 0).ThenBy(x => x.PrioOrder).ThenByDescending(x => x.ChangeStates.Max(cs => cs.CreatedAt)).ToList();
            }

            // TODO: dit moet eigenlijk verder naar boven in de query
            //if (states.Any())
            //{
            //    itemsQuery = itemsQuery.Where(x => states.Contains(x.ChangeStates.Single().IssueState));
            //}
            if (states.Any())
            {
                items = items.Where(x => states.Contains(x.State)).ToList();
            }

            var filterPresets = session.Query<FilterPreset>().Where(x => x.User == CurrentUser && (x.Project == null || x.Project.Id == project.Id)).ToList();
            var globalFilterPresets = session.Query<FilterPreset>().Where(x => x.User == null && (x.Project == null || x.Project.Id == project.Id)).ToList();

            PropertyBag.Add("items", items.ToList());
            PropertyBag.Add("project", project);
            PropertyBag.Add("selectedLabels", selectedLabels);
            PropertyBag.Add("notSelectedLabels", notSelectedLabels);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.VisibleForCustomer).ToList());
            PropertyBag.Add("customerLabels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer).ToList());
            PropertyBag.Add("states", states);
            PropertyBag.Add("query", query);
            PropertyBag.Add("filterPresets", filterPresets);
            PropertyBag.Add("globalFilterPresets", globalFilterPresets);
            PropertyBag.Add("selectedSorts", selectedSorts);
            PropertyBag.Add("allSorts", allSorts);
            PropertyBag.Add("allAdmins", session.Query<User>().Where(x => x.IsAdmin).ToList());
            PropertyBag.Add("selectedPickupbys", selectedPickupbys);
            PropertyBag.Add("pickupany", pickupany);
            PropertyBag.Add("pickupnone", pickupnone);
        }

        [MustHaveProject]
        public void View(string projectSlug, int issueId)
        {
            if (!string.IsNullOrEmpty(Request.UrlReferrer))
            {
                var referer = new Uri(Request.UrlReferrer);
                var routeMatch = RoutingModuleEx.Engine.FindMatch(referer.AbsolutePath,
                                                                  new RouteContext(Request, null, "/", new Hashtable()));
                if (routeMatch.Name == "issues") PropertyBag.Add("referer", referer.PathAndQuery);
            }

            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
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
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
            PropertyBag.Add("item", item);
            PropertyBag.Add("comments", item.Comments);
            PropertyBag.Add("datetime", DateTime.Now);
            CancelLayout();
        }

        [MustHaveProject]
        public void New(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            PropertyBag.Add("item", new Issue());
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug, int issueId)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            PropertyBag.Add("project", project);
            var item = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
            PropertyBag.Add("item", item);
            PropertyBag.Add("labels", CurrentUser.IsAdmin ? project.Labels : project.Labels.Where(l => l.ApplicableByCustomer));
        }

        [MustHaveProject]
        public void Save(string projectSlug, int issueId, string[] labels)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);

            var hash = string.Empty;
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
                hash = $"#issue{issue.Number}";
            }

            issue.Labels = labels.Select(label => session.Query<Label>().FirstOrDefault(l => l.Name == label)).ToList();

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(issue);
                transaction.Commit();
            }
            RedirectToUrl($"/project/{project.Slug}/issue/index{hash}");
        }

        [Admin]
        public void Pickup(string projectSlug, int issueId)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
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
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
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
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
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
        public void BookTime(string projectSlug, int issueId, string date, double hours, bool close)
        {
            RedirectToReferrer();
            if (hours <= 0.0)
            {
                Flash.Add("error", "Geen uren ingevuld, niets geboekt naar Freckle");
                RedirectToReferrer();
                return;
            }
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
            if (issue.IsArchived) return;
            var labels = project.Labels.Where(l => l.ToFreckle && issue.Labels.Select(label => label.Name).Contains(l.Name)).Select(l => l.Name).ToList();
            var description = string.Format("{0}, !!#{1} - {2}", string.Join(",", labels), issue.Number, issue.Name);
            var entry = new Entry()
                            {
                                Date = date,
                                Description = description,
                                Minutes = string.Format("{0}h", hours),
                                ProjectId = project.FreckleId,
                                User = CurrentUser.FreckleEmail
                            };
            if (entryClient.Post(entry))
            {
                Flash.Add("info", "Uren geboekt in Freckle");
                if (close)
                {
                    issue.Close(CurrentUser);
                    using (var tx = session.BeginTransaction())
                    {
                        session.SaveOrUpdate(issue);
                        tx.Commit();
                    }
                }
            }
            else
                Flash.Add("error", "Er is iets mis gegaan met boeken in Freckle");

        }

        [MustHaveProject]
        public void Close(string projectSlug, int issueId)
        {
            RedirectToReferrer();
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
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
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
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
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
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
            
            var issue = session.Query<Issue>().First(x => x.Project.Slug == projectSlug && x.Number == issueNumber);
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
            var issues = session.Query<Issue>().Where(x => x.Project.Slug == projectSlug && x.Pickups.Count == 0).ToList().Where(x => x.ChangeStates.Last().IssueState == IssueState.Open);
            return issues.ToDictionary(issue => issue.Number, issue => issue.PrioOrder);
        }

        [Admin]
        public void ExportImport(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

            PropertyBag.Add("project", project);
        }

        [Admin]
        public void ExportJson(string projectSlug, string[] selectedLabels, IssueState state, string issues)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

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

            var json = JsonConvert.SerializeObject(items);

            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = string.Format("issues_{0}_{1:yyyyMMdd_hhmm}.json", project.Name, DateTime.Now);

            Response.AppendHeader("content-disposition", string.Format("attachment; filename={0}", filename));
            Response.ContentType = "application/json";

            var byteArray = Encoding.Default.GetBytes(json);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }

        [Admin]
        public void ExportCsv(string projectSlug, string[] selectedLabels, IssueState state, string issues)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);

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

            var filename = string.Format("issues_{0}_{1:yyyyMMdd_hhmm}.csv", project.Name, DateTime.Now);

            Response.AppendHeader("content-disposition", string.Format("attachment; filename={0}", filename));
            Response.ContentType = "application/csv";

            var byteArray = Encoding.Default.GetBytes(csv);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }
    }
}