﻿namespace Gitle.Web.QueryParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using FluentNHibernate.Utils;
    using Model;
    using Model.Helpers;
    using NHibernate;
    using NHibernate.Linq;

    public class BookingQueryParser
    {
        private const string queryRegex = @"[a-zA-Z0-9_]+:(([a-zA-Z0-9-_,.]+)|('[a-zA-Z0-9_,. ]+'))";

        public IList<User> Users { get; set; } = new List<User>();
        public IList<Project> Projects { get; set; } = new List<Project>();
        public IList<Application> Applications { get; set; } = new List<Application>();
        public IList<Customer> Customers { get; set; } = new List<Customer>();
        public IList<Issue> Issues { get; set; } = new List<Issue>();

        public List<BookingGroup> GroupedBookings { get; set; }

        public List<Booking> AllBookings()
        {
            return GroupedBookings.SelectMany(x => x.Bookings).ToList();
        }

        public string GroupedBy { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now.StartOfMonth();
        public DateTime EndDate { get; set; } = DateTime.Now.EndOfMonth();

        public int MaxResults { get; set; } = 100;

        public int ResultCount { get; set; }
        public int TotalResultCount { get; set; }
        public int OmmitedResults => TotalResultCount - ResultCount;

        public Project SelectedProject;

        public string Query { get; set; }

        public Dictionary<string, string> AllGroupbys = new Dictionary<string, string>()
                            {
                                {"user", "Medewerker"},
                                {"project", "Project"},
                                {"application", "Applicatie"},
                                {"customer", "Klant"}
                            };


        public BookingQueryParser(ISession session, string query)
        {
            Query = query ?? string.Empty;

            var matches = Regex.Matches(Query, queryRegex);

            IList<string> userStrings = new List<string>();
            IList<string> projects = new List<string>();
            IList<string> applications = new List<string>();
            IList<string> customers = new List<string>();
            IList<string> issues = new List<string>();
            var searchQuery = Query;
            var take = MaxResults;
            var all = false;

            foreach (Match match in matches)
            {
                var parts = match.Value.Split(':');
                var value = parts[1].Replace("'", "");
                searchQuery = searchQuery.Replace(match.Value, "").Trim();
                switch (parts[0])
                {
                    case "user":
                        userStrings.Add(value);
                        break;
                    case "project":
                        projects.Add(value);
                        break;
                    case "application":
                        applications.Add(value);
                        break;
                    case "customer":
                        customers.Add(value);
                        break;
                    case "issue":
                        issues.Add(value);
                        break;
                    case "groupby":
                        GroupedBy = value;
                        break;
                    case "take":
                        if (value == "all")
                            all = true;
                        else
                            take = int.Parse(value);
                        break;
                    case "start":
                        DateTime startDate;
                        if(DateTime.TryParse(value, out startDate))
                            StartDate = startDate;
                        break;
                    case "end":
                        DateTime endDate;
                        if(DateTime.TryParse(value, out endDate))
                            EndDate = endDate;
                        break;
                }
            }

            var bookings = session.Query<Booking>().Where(x => x.IsActive && x.Date >= StartDate && x.Date <= EndDate);

            if (userStrings.Count > 0)
            {
                bookings = bookings.Where(x => x.User != null && (userStrings.Contains(x.User.Name) || userStrings.Contains(x.User.FullName)));
                foreach (var userString in userStrings)
                {
                    Users.Add(session.Query<User>().FirstOrDefault(x => x.Name == userString || x.FullName == userString));
                }
            }

            if (projects.Count > 0)
            {
                bookings = bookings.Where(x => x.Project != null && projects.Contains(x.Project.Slug));
                foreach (var project in projects)
                {
                    Projects.Add(session.SlugOrDefault<Project>(project));
                }
            }

            if (applications.Count > 0)
            {
                bookings = bookings.Where(x => x.Project != null && x.Project.Application != null && applications.Contains(x.Project.Application.Slug));
                foreach (var application in applications)
                {
                    Applications.Add(session.SlugOrDefault<Application>(application));
                }
            }

            if (customers.Count > 0)
            {
                bookings = bookings.Where(x => x.Project != null && x.Project.Application != null && x.Project.Application.Customer != null && customers.Contains(x.Project.Application.Customer.Slug));
                foreach (var customer in customers)
                {
                    Customers.Add(session.SlugOrDefault<Customer>(customer));
                }
            }

            if (issues.Count > 0 && Projects.Count == 1) //Er mag maar één project selecteerd zijn, anders wordt het heel onoverzichtelijk met dubbele Issue Numbers
            {
                SelectedProject = Projects.First();
                bookings = bookings.Where(x => x.Project != null && x.Project.Application != null && issues.Contains(x.Issue.Number.ToString()));
                foreach (var issue in issues)
                {
                    Issues.Add(session.Query<Issue>().FirstOrDefault(x => x.Number.ToString().Equals(issue) && x.Project == SelectedProject));
                }
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                bookings = bookings.Where(x => x.Comment.Contains(searchQuery));
            }

            bookings = bookings.OrderByDescending(x => x.Date);

            TotalResultCount = bookings.Count();

            if (!all)
            {
                bookings = bookings.Take(take);
            }

            ResultCount = TotalResultCount > take && !all ? take : TotalResultCount;

            switch (GroupedBy)
            {
                case "project":
                    GroupedBookings = bookings.OrderBy(x => x.Project.Name).ToList().GroupBy(x => x.Project).Select(x => new BookingGroup(x.Key.Name, x.ToList())).ToList();
                    break;
                case "user":
                    GroupedBookings = bookings.OrderBy(x => x.User.FullName).ToList().GroupBy(x => x.User).Select(x => new BookingGroup(x.Key.FullName, x.ToList())).ToList();
                    break;
                case "application":
                    GroupedBookings = bookings.Where(x => x.Project.Application != null).OrderBy(x => x.Project.Application.Name).ToList().GroupBy(x => x.Project.Application).Select(x => new BookingGroup(x.Key.Name, x.ToList())).ToList();
                    break;
                case "customer":
                    GroupedBookings = bookings.Where(x => x.Project.Application != null && x.Project.Application.Customer != null).OrderBy(x => x.Project.Application.Customer.Name).ToList().GroupBy(x => x.Project.Application.Customer).Select(x => new BookingGroup(x.Key.Name, x.ToList())).ToList();
                    break;
                default:
                    GroupedBookings = new List<BookingGroup> { new BookingGroup("Alle uren", bookings.ToList()) };
                    break;
            }
        }

        public class BookingGroup
        {
            public BookingGroup(string title, List<Booking> bookings)
            {
                Bookings = bookings;
                Title = title;
            }

            public string Title { get; set; }
            public List<Booking> Bookings { get; set; }

            public double Minutes
            {
                get { return Bookings.Sum(x => x.Minutes); }
            }
            public virtual double Hours => Minutes / 60.0;

            public virtual string Time => $"{Math.Floor(Hours)}:{Minutes - (Math.Floor(Hours)*60):00}";
        }
    }
}