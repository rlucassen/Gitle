﻿namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Gitle.Model.Enum;
    using Helpers;
    using Model;
    using NHibernate;
    using NHibernate.Linq;

    public class SearchController : BaseController
    {
        public SearchController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        [return: JSONReturnBinder]
        public object Index(string query)
        {
            var suggestions = new List<Suggestion>();

            var users = session.Query<User>().Where(x => x.FullName.Contains(query) && !x.IsAdmin && x.IsActive);
            suggestions.AddRange(users.Select(x => new Suggestion($"Persoon: {x.FullName}", $"/user/{x.Id}/view")));

            var customers = session.Query<Customer>().Where(x => x.Name.Contains(query) && x.IsActive);
            suggestions.AddRange(customers.Select(x => new Suggestion($"Klant: {x.Name}", $"/customer/{x.Slug}/view")));

            var projects = session.Query<Project>().Where(x => (x.Name.Contains(query) || x.Number.ToString().Contains(query)) && !x.Closed && x.IsActive);
            suggestions.AddRange(projects.Select(x => new Suggestion($"Project: {x.Name}", $"/project/{x.Slug}/view")));
            suggestions.AddRange(projects.Select(x => new Suggestion("Taken: " + x.Name, $"/project/{x.Slug}/issues")));

            var applications = session.Query<Application>().Where(x => x.Name.Contains(query) && x.IsActive);
            suggestions.AddRange(applications.Select(x => new Suggestion($"Application: {x.Name}", $"/application/{x.Slug}/view")));

            return new {query = query, suggestions = suggestions };
        }
    }

    public class Suggestion
    {
        public Suggestion(string value, string data)
        {
            this.value = value;
            this.data = data;
        }
        public Suggestion(string value, string data, string extraValue, string extraValue2 = "", string extraValue3 = "")
        {
            this.value = value;
            this.data = data;
            this.extraValue = extraValue;
            this.extraValue2 = extraValue2;
            this.extraValue3 = extraValue3;
        }
        public string value;
        public string data;
        public string extraValue;
        public string extraValue2;
        public string extraValue3;
    }
}