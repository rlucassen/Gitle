﻿namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Model;
    using Helpers;
    using Model.Enum;
    using Model.Helpers;
    using Model.Interfaces.Service;
    using NHibernate;
    using NHibernate.Linq;
    using ViewModel;

    public class ProjectController : SecureController
    {
        private readonly IProjectNumberService _projectNumberService;

        public ProjectController(ISessionFactory sessionFactory, IProjectNumberService projectNumberService) : base(sessionFactory)
        {
            _projectNumberService = projectNumberService;
        }

        public void Index(string customerSlug, string applicationSlug)
        {
            if (CurrentUser.Projects.Count == 1 && !CurrentUser.IsAdmin)
                RedirectUsingNamedRoute("issues", new {projectSlug = CurrentUser.Projects.First().Project.Slug});

            var projects = session.Query<Project>().Where(x => x.IsActive);

            if (!CurrentUser.IsDanielle)
            {
                projects = projects.Where(x => x.Type != ProjectType.Administration);
            }
            
            if (!CurrentUser.IsAdmin)
            {
                projects = projects.Where(p => p.Users.Any(x => x.User == CurrentUser));
            }
            else
            {
                if (!string.IsNullOrEmpty(applicationSlug))
                {
                    var application = session.Slug<Application>(applicationSlug);
                    PropertyBag.Add("application", application);
                    projects = projects.Where(x => x.Application == application);
                }
                else if (!string.IsNullOrEmpty(customerSlug))
                {
                    var customer = session.Slug<Customer>(customerSlug);
                    PropertyBag.Add("customer", customer);
                    projects = projects.Where(x => x.Application != null && x.Application.Customer == customer);
                }
            }

            PropertyBag.Add("items", projects);
        }

        [return: JSONReturnBinder]
        public object List(int start, int length, int draw, string orderColumn, string orderDir, bool closed)
        {
            var projects = session.Query<Project>().Where(x => x.IsActive);
            var recordsTotal = projects.Count();

            if (!string.IsNullOrEmpty(orderColumn))
            {
                projects = projects.OrderByProperty(orderColumn, orderDir != "asc");
            }

            if (!closed)
            {
                projects = projects.Where(x => !x.Closed);
            }

            if (!CurrentUser.IsAdmin)
            {
                projects = projects.Where(p => p.Users.Any(x => x.User == CurrentUser));
            }

            var recordsFiltered = projects.Count();

            if (length > 0)
            {
                projects = projects.Skip(start).Take(length);
            }

            var data = projects.Select(x => new ProjectListViewModel(x));

            return new { draw, start, length, recordsTotal, recordsFiltered, data };
        }

        [MustHaveProject]
        public void View(string projectSlug)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            PropertyBag.Add("project", project);

            var application = session.Query<Application>().FirstOrDefault(x => x.Projects.Contains(project));

            if (CurrentUser.IsAdmin)
            {
                var bookedTime = project.BillableMinutes/60.0;
                var totalTime = project.BudgetMinutes/60.0;

                var overbooked = false;
                var bookedPercentage = bookedTime*100.0/totalTime;
                if (bookedPercentage > 100)
                {
                    bookedPercentage = 100;
                    overbooked = true;
                }

                PropertyBag.Add("overbooked", overbooked);
                PropertyBag.Add("bookedTime", bookedTime);
                PropertyBag.Add("bookedPercentage", bookedPercentage);
                PropertyBag.Add("totalTime", totalTime);
            }
            var issues = session.Query<Issue>().Where(x => x.Project == project).ToList();
            var doneTime = issues.Where(i => !i.IsOpen).Sum(i => i.TotalHours);
            var totalIssueTime = issues.Sum(i => i.TotalHours);
            var donePercentage = doneTime*100.0/totalIssueTime;
            
            PropertyBag.Add("doneTime", doneTime);
            PropertyBag.Add("donePercentage", donePercentage);
            PropertyBag.Add("totalIssueTime", totalIssueTime);
            PropertyBag.Add("application", application);
            PropertyBag.Add("customers", project.Users.Where(up => !up.User.IsAdmin).ToList());
            PropertyBag.Add("developers", project.Users.Where(up => up.User.IsAdmin).ToList());
        }

        [Admin]
        public void New(string applicationSlug, string customerSlug)
        {
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            var applications = session.Query<Application>().Where(x => x.IsActive);
            if (!string.IsNullOrEmpty(customerSlug))
            {
                applications = applications.Where(x => x.Customer.Slug == customerSlug);
            }
            PropertyBag.Add("applications", applications.OrderBy(x => x.Name));
            if (!string.IsNullOrEmpty(applicationSlug))
            {
                PropertyBag.Add("applicationId", session.Slug<Application>(applicationSlug));
            }
            PropertyBag.Add("types", EnumHelper.ToList(typeof(ProjectType)));
            PropertyBag.Add("item", new Project() {Number = _projectNumberService.GetNextProjectNumber()});
            RenderView("edit");
        }

        [Admin]
        public void Edit(string projectSlug)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            PropertyBag.Add("customers", session.Query<Customer>().Where(x => x.IsActive).ToList());
            PropertyBag.Add("applications", session.Query<Application>().Where(x => x.IsActive).OrderBy(x => x.Name));
            PropertyBag.Add("applicationId", session.Query<Application>().Where(x => x.Projects.Contains(project)));
            PropertyBag.Add("types", EnumHelper.ToList(typeof(ProjectType)));
            PropertyBag.Add("item", project);
            PropertyBag.Add("customerId", project.Customer?.Id);
        }

        [Admin]
        public void Delete(string projectSlug)
        {
            var project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Slug == projectSlug);
            project.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(project);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [Admin]
        public void Save(string projectSlug, long applicationId)
        {
            var item = session.SlugOrDefault<Project>(projectSlug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Project>("item");
            }

            if (applicationId > 0)
            {
                var application = session.Get<Application>(applicationId);
                item.Application = application;
                application.Projects.Add(item);
                session.SaveOrUpdate(application);
            }

            var labels = BindObject<Label[]>("label");

            var labelsToDelete = item.Labels.Where(l => !labels.Select(x => x.Id).Contains(l.Id)).ToList();

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                foreach (var label in labels.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                {
                    label.Project = item;
                    session.Merge(label);
                }
                foreach (var label in labelsToDelete)
                {
                    item.Labels.Remove(label);
                    session.Delete(label);
                }
                tx.Commit();
            }

            RedirectToUrl("/projects");
        }

        [MustHaveProject]
        public void AddLabel(string projectSlug, string issues, string label)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            var issueIds = issues.Split(',');
            var realLabel = session.Query<Label>().FirstOrDefault(x => x.Name == label);
            if (!realLabel.ApplicableByCustomer && !CurrentUser.IsAdmin)
            {
                RedirectToReferrer();
                return;
            }

            using (var transaction = session.BeginTransaction())
            {
                foreach (var issueId in issueIds.Select(int.Parse))
                {
                    var issue = session.Query<Issue>().FirstOrDefault(x => x.Number == issueId && x.Project == project);
                    if (!issue.Labels.Contains(realLabel)) issue.Labels.Add(realLabel);
                    session.SaveOrUpdate(issue);
                }

                transaction.Commit();
            }

            RedirectToReferrer();
        }

        [MustHaveProject]
        public void ChangeState(string projectSlug, string issues, IssueState state)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            if (project.Closed)
            {
                throw new ProjectClosedException(project);
            }
            var issueIds = issues.Split(',');

            using (var transaction = session.BeginTransaction())
            {
                foreach (var issueId in issueIds.Select(int.Parse))
                {
                    var issue = session.Query<Issue>().FirstOrDefault(i => i.Number == issueId && i.Project == project);
                    issue.ChangeState(CurrentUser, state);
                    session.SaveOrUpdate(issue);
                }

                transaction.Commit();
            }

            RedirectToReferrer();
        }

        [Admin]
        public void Comments(string projectSlug, string comment)
        {
            var item = session.SlugOrDefault<Project>(projectSlug);

            item.Comments = comment;

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RenderText(comment);
        }

        [Admin]
        public void DeleteDocument(string projectSlug, long id)
        {
            var item = session.SlugOrDefault<Project>(projectSlug);
            var document = session.Get<Document>(id);

            item.Documents.Remove(document);


            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [return: JSONReturnBinder]
        public object Autocomplete(string query)
        {
            var suggestions = new List<Suggestion>();
            var projects = session.Query<Project>().Where(x => x.IsActive && !x.Closed);

            if (!CurrentUser.IsDanielle)
            {
                projects = projects.Where(x => x.Type != ProjectType.Administration);
            }

            if (query != null)
            {
                projects = projects.Where(p => p.Name.Contains(query) || p.Number.ToString().Contains(query) || (p.Application != null && (p.Application.Name.Contains(query) || (p.Application.Customer != null && p.Application.Customer.Name.Contains(query)))));
            }
            projects = projects.OrderBy(x => x.Name);
            suggestions.AddRange(projects.ToList().Select(x => new Suggestion(x.CompleteName, x.Id.ToString(), x.TicketRequiredForBooking ? "ticketRequired": string.Empty, x.Unbillable ? "unbillable":string.Empty, x.Slug)));
            return new {query, suggestions };
        }

        [return: JSONReturnBinder]
        public object CheckProjectName(string name, long projectId)
        {
            var validName = !session.Query<Project>().Any(x => x.IsActive && x.Slug == name.Slugify() && x.Id != projectId);
            var message = "Voer een naam in";
            if (!validName)
            {
                message = "Deze naam is al in gebruik, kies een andere";
            }
            return new { success = validName, message = message };
        }
    }
}