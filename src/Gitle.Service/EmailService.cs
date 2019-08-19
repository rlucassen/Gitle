﻿namespace Gitle.Service
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using Castle.Core.Logging;
    using Castle.Core.Smtp;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Views.Brail;
    using Gitle.Model.Enum;
    using Model;
    using Model.Interfaces.Model;
    using Model.Interfaces.Service;
    using NHibernate;

    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;
        private readonly bool _testMode;
        private readonly string _sourceAddress;
        private readonly DefaultSmtpSender _defaultSmtpSender;

        private static readonly FileAssemblyViewSourceLoader ViewSourceLoader =
            new FileAssemblyViewSourceLoader(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views"), "Mail"));

        private static readonly StandaloneBooViewEngine StandaloneBooViewEngine = new StandaloneBooViewEngine(ViewSourceLoader, null);

        private readonly ISession _session;

        public EmailService(string hostname, string sourceAddress, bool testMode, ILogger logger, ISessionFactory sessionFactory)
        {
            _defaultSmtpSender = new DefaultSmtpSender(hostname);

            this._sourceAddress = sourceAddress;
            this._testMode = testMode;
            this._logger = logger;
            this._session = sessionFactory.GetCurrentSession();
        }
        #region IEmailService Members

        public void SendIssueActionNotification(IIssueAction action)
        {
            if (action.Issue.Project.IsMuted) return;

            if (action is ChangeState)
                action = _session.Get<ChangeState>(((ChangeState)action).Id);
            var project = action.Issue.Project;
            IList<User> users = (from userProject in project.Users where userProject.Notifications && (!userProject.OnlyOwnIssues || action.Issue.CreatedBy == userProject.User) && userProject.User != action.User && userProject.IsActive && userProject.User.IsActive select userProject.User).ToList();

            if ((action as ChangeState)?.IssueState == IssueState.Open)
            {
                var servicedesk = new User()
                {
                    IsAdmin = true,
                    EmailAddress = "servicedesk@auxilium.nl"
                };
                users.Add(servicedesk);
            }

            if (action is Comment)
            {
                var userlist = users.ToList();

                if ((action as Comment).IsInternal)
                {
                    foreach (var user in userlist)
                    {
                        if (!user.CanBookHours && !user.IsAdmin)
                        {
                            users.Remove(user);
                        }
                    }
                }
                else
                {
                    foreach (var user in userlist)
                    {
                        if (user.CanBookHours || user.IsAdmin)
                        {
                            users.Remove(user);
                        }
                    }
                }
            }

            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.EmailAddress))
                {
                    var message = new MailMessage(_sourceAddress, user.EmailAddress)
                    {
                        Subject = $"Gitle: {action.EmailSubject} - {action.Issue.Project.Name}",
                        IsBodyHtml = true
                    };

                    message.Body = GetBody("issue-action", new Hashtable { { "item", action }, { "user", user } });

                    SendMessage(message);
                }
            }
        }

        public void SendHandOverNotification(HandOver handOver)
        {
            if (!string.IsNullOrEmpty(handOver.User.EmailAddress))
            {
                var message = new MailMessage(_sourceAddress, handOver.User.EmailAddress)
                {
                    Subject = $"Gitle: {handOver.EmailSubject} - {handOver.Issue.Project.Name}",
                    IsBodyHtml = true
                };

                message.Body = GetBody("issue-action", new Hashtable { { "item", handOver }, { "user", handOver.User } });

                SendMessage(message);
            }
        }

        public void SendPasswordLink(User user)
        {
            var message = new MailMessage(_sourceAddress, user.EmailAddress)
            {
                Subject = string.Format("Gitle: Aanvraag wachtwoord wijzigen"),
                IsBodyHtml = true
            };

            message.Body = GetBody("password", new Hashtable { { "user", user } });

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
            if (_testMode)
            {
                _logger.DebugFormat("Email service staat in testmodus");
                _logger.DebugFormat("Bericht: {0}, naar: {1}", message.Subject, message.To);
                _logger.Debug(message.Body);
            }
            else
            {
                _defaultSmtpSender.Send(message);
            }
        }

    }
}