namespace Gitle.Web
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using Boo.Lang;
    using Castle.Windsor;
    using Model.Interfaces.Model;
    using Model.Interfaces.Service;
    using NHibernate;
    using NHibernate.Type;

    public class GitleInterceptor : EmptyInterceptor
    {
        private readonly IWindsorContainer container;
        private IEmailService emailService;

        private IList<object> savedObjects = new List();

        public GitleInterceptor(IWindsorContainer container)
        {
            this.container = container;
        }

        public IEmailService EmailService
        {
            get { return emailService = container.Resolve<IEmailService>(); }
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            if(entity is IIssueAction)
                savedObjects.Add(entity);
            return true;
        }

        public override void PostFlush(ICollection entities)
        {
            foreach (var entity in savedObjects)
            {
                if (entity is IIssueAction)
                    EmailService.SendIssueActionNotification((IIssueAction) entity);
            }
            savedObjects.Clear();
        }
    }
}