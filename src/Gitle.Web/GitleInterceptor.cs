namespace Gitle.Web
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Boo.Lang;
    using Castle.Windsor;
    using Model;
    using Model.Interfaces.Model;
    using Model.Interfaces.Service;
    using NHibernate;
    using NHibernate.Type;

    public class GitleInterceptor : EmptyInterceptor
    {
        private readonly IWindsorContainer container;
        private IEmailService emailService;

        private IList<object> savedObjects = new List();
        private IList<object> deletedObjects = new List();

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

        public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            if (entity is Document)
                deletedObjects.Add(entity);
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
        {
            if (entity is Comment)
            {
                var i = propertyNames.ToList().IndexOf("IsInternal");
                var previousValue = previousState[i];
                var currentValue = currentState[i];

                if (currentValue.Equals(false))
                {
                savedObjects.Add(entity);
                }
            }
            return base.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types);
        }

        public override void PostFlush(ICollection entities)
        {
            foreach (var entity in savedObjects)
            {
                var action = entity as IIssueAction;
                if (action != null)
                    EmailService.SendIssueActionNotification(action);
            }
            savedObjects.Clear();

            foreach (var entity in deletedObjects)
            {
                var document = entity as Document;
                if (document == null) continue;
                var path = Path.Combine(ConfigurationManager.AppSettings["fileUpload"], document.Path.Replace("/Public/", ""));
                var fileInfo = new FileInfo(path);
                if(fileInfo.Exists) fileInfo.Delete();
            }
            deletedObjects.Clear();
        }
    }
}