// ReSharper disable once IdentifierTypo
namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Gitle.Model;
    using Gitle.Model.Enum;
    using Gitle.Web.Helpers;
    using Model.Helpers;
    using NHibernate;
    using NHibernate.Linq;

    public class InstallationController : SecureController
    {
        public InstallationController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index()
        {
            var items = session.Query<Installation>().Where(x => x.IsActive).ToList();

            PropertyBag.Add("items", items);
        }

        [Admin]
        public void Edit(string installationSlug)
        {
            var installation = session.SlugOrDefault<Installation>(installationSlug);
            var installationTypes = EnumHelper.ToDictionary(typeof(InstallationType));
            var types = installationTypes.Where(t => new[] { InstallationType.Live, InstallationType.Acceptance, InstallationType.Demo }.Contains((InstallationType)t.Key)).ToList();

            PropertyBag.Add("applications", session.Query<Application>().Where(x => x.IsActive).OrderBy(x => x.Name));
            PropertyBag.Add("applicationId", installation?.Application?.Id);
            PropertyBag.Add("servers", session.Query<Server>().Where(x => x.IsActive).OrderBy(x => x.Name));
            PropertyBag.Add("serverId", installation?.Server?.Id);
            PropertyBag.Add("installationTypes", types);
            PropertyBag.Add("item", installation ?? new Installation());
            RenderView("edit");
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new Installation());
            RenderView("edit");
        }

        public string MakeUrlComplete(string url)
        {
            var ignoreString = new List<string> { "http://", "https://" };
            var ignore = false;
            
            foreach (var key in ignoreString)
            {
                if (url.Substring(0, key.Length) == key)
                    ignore = true;
            }

            if (!ignore)
                url = "https://" + url;

            return url;
        }

        [Admin]
        public void Save(string installationSlug, long applicationId, long serverId)
        {
            var item = session.SlugOrDefault<Installation>(installationSlug);
            var application = session.Get<Application>(applicationId);
            var server = session.Get<Server>(serverId);

            if (item != null)
                BindObjectInstance(item, "item");
            else
                item = BindObject<Installation>("item");

            item.Application = application;
            item.Server = server;
            item.Url = MakeUrlComplete(item.Url);
            item.Slug = $"{application.Name}-{item.InstallationType.ToString()}".Slugify();

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(item);
                tx.Commit();
            }

            RedirectToUrl("/installation");
        }

        [Admin]
        public void Delete(string installationSlug)
        {
            var installation = session.SlugOrDefault<Installation>(installationSlug);
            installation.Deactivate();
            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(installation);
                tx.Commit();
            }
            RedirectToReferrer();
        }

        [return: JSONReturnBinder]
        public object CheckInstallationName(string name, long installationId)
        {
            var validName = !session.Query<Installation>().Any(x => x.IsActive && x.Slug == name.Slugify() && x.Id != installationId);
            return new { success = validName, message = validName ? "" : "Er bestaat al een installatie met deze naam! Kies een ander naam!" };
        }

        [return: JSONReturnBinder]
        public object CheckUrl(string link, long installationId)
        {
            link = MakeUrlComplete(link);

            var validUrl = !session.Query<Installation>().Any(x => x.IsActive && x.Url == link && x.Id != installationId);
            return new { success = validUrl, message = validUrl ? "" : "Er bestaat al een installatie met deze url!" };
        }
    }
}