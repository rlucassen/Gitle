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
    using Model.Interfaces.Repository;
    using Model.Interfaces.Service;

    public class EmailService : IEmailService
    {
        private readonly ILogger logger;
        private readonly bool testMode;
        private readonly string sourceAddress;
        private readonly DefaultSmtpSender defaultSmtpSender;

        private static readonly FileAssemblyViewSourceLoader ViewSourceLoader =
            new FileAssemblyViewSourceLoader(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views"), "Mail"));

        private static readonly StandaloneBooViewEngine StandaloneBooViewEngine = new StandaloneBooViewEngine(ViewSourceLoader, null);

        private readonly IProjectRepository projectRepository;

        public EmailService(string hostname, string sourceAddress, bool testMode, ILogger logger, IProjectRepository projectRepository)
        {
            defaultSmtpSender = new DefaultSmtpSender(hostname);

            this.sourceAddress = sourceAddress;
            this.testMode = testMode;
            this.logger = logger;
            this.projectRepository = projectRepository;
        }

        #region IEmailService Members

        public void SendHookNotification(HookPayload hookPayload)
        {
            if (string.IsNullOrEmpty(hookPayload.Issue.RepoName) || hookPayload.Issue.Milestone == null)
            {
                return;
            }

            var project = projectRepository.FindByRepoAndMilestone(hookPayload.Issue.RepoName, hookPayload.Issue.Milestone.Number).FirstOrDefault();

            IList<User> users = (from userProject in project.Users where userProject.Notifications select userProject.User).ToList();

            if (hookPayload.Comment != null)
            {
                SendCommentNotification(hookPayload, project, users);
            }
            else
            {
                if(hookPayload.Action == "opened" || hookPayload.Action == "reopened")
                    SendIssueNotification(hookPayload, project, users);
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

        private void SendCommentNotification(HookPayload hookPayload, Project project, IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                var message = new MailMessage(sourceAddress, user.EmailAddress)
                {
                    Subject = string.Format("Gitle: Nieuwe reactie bij project {0}", project.Name),
                    IsBodyHtml = true
                };

                message.Body = GetBody("comment",
                                       new Hashtable {{"item", hookPayload}, {"project", project}, {"user", user}});

                if(hookPayload.Comment.Name != user.FullName && hookPayload.Comment.Name != user.GitHubUsername)
                    SendMessage(message);
            }
        }

        private void SendIssueNotification(HookPayload hookPayload, Project project, IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                var message = new MailMessage(sourceAddress, user.EmailAddress)
                {
                    Subject = string.Format("Gitle: Taak status gewijzigd bij project {0}", project.Name),
                    IsBodyHtml = true
                };

                message.Body = GetBody("issue",
                                       new Hashtable {{"item", hookPayload}, {"project", project}, {"user", user}});

                SendMessage(message);
            }
        }

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