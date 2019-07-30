namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Descriptors;
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
            PropertyBag.Add("hostings", session.Query<Hosting>().Where(x => x.IsActive).ToList());
            RenderView("edit");
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new Server());
            RenderView("edit");
        }

        [Admin]
        public void Save(string slug = "")
        {
            var item = session.SlugOrDefault<Server>(slug);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Server>("item");
            }

            var hosting = session.Get<Hosting>(long.Parse(Params["hostingId"]));

            using (var tx = session.BeginTransaction())
            {
                item.Hosting = hosting;
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

        [return: JSONReturnBinder]
        public object CheckServerName(string name, long serverId)
        {
            var validName = !session.Query<Server>().Any(x => x.IsActive && x.Slug == name.Slugify() && x.Id != serverId);
            return new { success = validName, message = validName ? "": "Er bestaat al een server met deze naam! Kies een ander naam!" };
        }
    }
}