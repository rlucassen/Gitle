namespace Gitle.Web.Controllers
{
    using System.Linq;
    using Gitle.Model;
    using Gitle.Model.Helpers;
    using Gitle.Web.Helpers;
    using NHibernate;
    using NHibernate.Linq;

    public class ServerController : SecureController
    {
        public ServerController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", session.Query<Server>().Where(x => x.IsActive).ToList());
        }

        [Admin]
        public void View(string serverSlug)
        {
            var server = session.SlugOrDefault<Server>(serverSlug);
            PropertyBag.Add("item", server);
        }

        [Admin]
        public void Edit(string serverSlug)
        {
            var server = session.SlugOrDefault<Server>(serverSlug);
            PropertyBag.Add("item", (server != null) ? server : new Server());
            RenderView("edit");
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new Server());
            RenderView("edit");
        }

        [Admin]
        public void Save(string serverSlug)
        {
            var item = session.SlugOrDefault<Server>(serverSlug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Server>("item");
            }

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/server");
        }

        [Admin]
        public void Delete(string serverSlug)
        {
            var server = session.SlugOrDefault<Server>(serverSlug);
            server.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(server);
                tx.Commit();
            }
            RedirectToReferrer();
        }
    }
}