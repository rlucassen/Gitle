namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using Admin;
    using Clients.GitHub.Interfaces;
    using Clients.GitHub.Models;
    using Model;
    using Model.Interfaces.Repository;
    using Helpers;

    public class IssueController : SecureController
    {
        private readonly IIssueClient client;
        private readonly ICommentClient commentClient;
        private readonly IProjectRepository repository;

        public IssueController(IProjectRepository repository, IIssueClient client, ICommentClient commentClient)
        {
            this.repository = repository;
            this.client = client;
            this.commentClient = commentClient;
        }

        public void Index(int projectId)
        {
            Project project = repository.Get(projectId);
            List<Issue> items = client.List(project.Repository, project.Milestone);
            PropertyBag.Add("items", items);
            PropertyBag.Add("project", project);
        }

        public void View(int projectId, int issueId)
        {
            Project project = repository.Get(projectId);
            PropertyBag.Add("project", project);
            Issue item = client.Get(project.Repository, issueId);
            PropertyBag.Add("item", item);
            List<Comment> comments = commentClient.List(project.Repository, issueId);
            PropertyBag.Add("comments", comments);
        }

        public void New(int projectId)
        {
            PropertyBag.Add("project", repository.Get(projectId));
            PropertyBag.Add("item", new Issue());
            RenderView("edit");
        }

        [Admin]
        public void Edit(int projectId, int issueId)
        {
            Project project = repository.Get(projectId);
            PropertyBag.Add("project", project);
            Issue item = client.Get(project.Repository, issueId);
            PropertyBag.Add("item", item);
        }

        public void Save(int projectId)
        {
            var issue = BindObject<Issue>("item");
            if (!CurrentUser.IsAdmin)
            {
                issue.CustomerIssue = true;
                issue.Accepted = true;
            }
            Project project = repository.Get(projectId);
            if (issue.Number > 0)
            {
                client.Patch(project.Repository, issue.Number, issue);
            }
            else
            {
                client.Post(project.Repository, issue);
            }
            RedirectToUrl(string.Format("/project/{0}/issue/index", project.Id));
        }

        public void AddComment(int projectId, int issueId, string body)
        {
            Project project = repository.Get(projectId);
            var comment = new Comment
                              {
                                  Body = string.Format("{0}: {1}", CurrentUser.Name, body)
                              };
            commentClient.Post(project.Repository, issueId, comment);
            RedirectToReferrer();
        }

        public void Accept(int projectId, int issueId)
        {
            Project project = repository.Get(projectId);
            Issue issue = client.Get(project.Repository, issueId);
            issue.Accepted = true;
            client.Patch(project.Repository, issueId, issue);
            RedirectToReferrer();
        }

        [Admin]
        public void Invoice(int projectId, int[] issueIds)
        {
            Project project = repository.Get(projectId);
            foreach (int issueId in issueIds)
            {
                Issue issue = client.Get(project.Repository, issueId);
                issue.Invoiced = true;
                client.Patch(project.Repository, issueId, issue);
            }
        }
    }
}