namespace Gitle.Service
{
    using System.Linq;
    using Model;
    using Model.Interfaces.Service;
    using NHibernate;
    using NHibernate.Linq;

    public class SettingService : ISettingService
    {
        protected ISessionFactory SessionFactory { get; }

        public SettingService(ISessionFactory sessionFactory)
        {
            SessionFactory = sessionFactory;
        }

        public Setting Load()
        {
            var session = SessionFactory.GetCurrentSession();
            var setting = session.Query<Setting>().SingleOrDefault(x => x.IsActive) ?? new Setting();
            session.SaveOrUpdate(setting);
            return setting;
        }
    }
}