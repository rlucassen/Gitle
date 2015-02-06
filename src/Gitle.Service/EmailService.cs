namespace Gitle.Service
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using Castle.Core.Logging;
    using Castle.Core.Smtp;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Views.Brail;
    using Clients.GitHub.Models.Hooks;
    using Model;
    using Model.Interfaces.Model;
    using Model.Interfaces.Service;
    using NHibernate;

    public class EmailService : IEmailService
    {
        private readonly ILogger logger;
        private readonly bool testMode;
        private readonly string sourceAddress;
        private readonly DefaultSmtpSender defaultSmtpSender;

        private static readonly FileAssemblyViewSourceLoader ViewSourceLoader =
            new FileAssemblyViewSourceLoader(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views"), "Mail"));

        private static readonly StandaloneBooViewEngine StandaloneBooViewEngine = new StandaloneBooViewEngine(ViewSourceLoader, null);

        private readonly ISession session;

        public EmailService(string hostname, string sourceAddress, bool testMode, ILogger logger, ISessionFactory sessionFactory)
        {
            defaultSmtpSender = new DefaultSmtpSender(hostname);

            this.sourceAddress = sourceAddress;
            this.testMode = testMode;
            this.logger = logger;
            this.session = sessionFactory.GetCurrentSession();
        }
        #region IEmailService Members

        public void SendIssueActionNotification(IIssueAction action)
        {
            if (action is ChangeState)
                action = session.Get<ChangeState>(((ChangeState)action).Id);
            var project = action.Issue.Project;
            IList<User> users = (from userProject in project.Users where userProject.Notifications && userProject.User != action.User select userProject.User).ToList();

            foreach (var user in users)
            {
                var message = new MailMessage(sourceAddress, user.EmailAddress)
                {
                    Subject =
                        string.Format("Gitle: {0} - {1}",
                                        action.EmailSubject, action.Issue.Project.Name),
                    IsBodyHtml = true
                };

                message.Body = GetBody("issue-action", new Hashtable { { "item", action }, { "user", user } });

                SendMessage(message);
            }
        }

        public void SendPasswordLink(User user)
        {
            var message = new MailMessage(sourceAddress, user.EmailAddress)
            {
                Subject = string.Format("Gitle: Aanvraag wachtwoord wijzigen"),
                IsBodyHtml = true
            };

            message.Body = GetBody("password",
                                   new Hashtable { { "user", user } });

            SendMessage(message);
        }

        #endregion

        private static string GetBody(string template, IDictionary parameters)
        {
            string body;
            using (var writer = new StringWriter())
            {
                StandaloneBooViewEngine.Process(template, writer, parameters);
                body = writer.GetStringBuilder().ToString();
            }
            return body;
        }

        private void SendMessage(MailMessage message)
        {
            if (testMode)
            {
                logger.DebugFormat("Email service staat in testmodus");
                logger.DebugFormat("Bericht: {0}, naar: {1}", message.Subject, message.To);
                logger.Debug(message.Body);
            }
            else
            {
                defaultSmtpSender.Send(message);
            }
        }

    }
}