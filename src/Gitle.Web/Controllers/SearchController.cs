namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Model;
    using NHibernate;
    using NHibernate.Linq;

    public class SearchController : BaseController
    {
        private readonly ISession session;

        public SearchController(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.GetCurrentSession();
        }

        [return: JSONReturnBinder]
        public object Index(string query)
        {
            var suggestions = new List<Suggestion>();

            var users = session.Query<User>().Where(x => x.FullName.Contains(query) && !x.IsAdmin);
            suggestions.AddRange(users.Select(x => new Suggestion($"Persoon: {x.FullName}", $"/user/{x.Id}/view")));

            var customers = session.Query<Customer>().Where(x => x.Name.Contains(query));
            suggestions.AddRange(customers.Select(x => new Suggestion($"Klant: {x.Name}", $"/customer/{x.Slug}/view")));

            var projects = session.Query<Project>().Where(x => x.Name.Contains(query));
            suggestions.AddRange(projects.Select(x => new Suggestion($"Project: {x.Name}", $"/project/{x.Slug}/view")));


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
        public string value;
        public string data;
    }
}