namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Clients.GitHub.Interfaces;
    using Model;
    using Model.Enum;
    using NHibernate;
    using NHibernate.Linq;
    using Comment = Clients.GitHub.Models.Comment;
    using Issue = Clients.GitHub.Models.Issue;
    using Label = Clients.GitHub.Models.Label;

    public class ImportController : SecureController
    {
        private readonly ICommentClient commentClient;
        private readonly IIssueClient issueClient;

        public ImportController(ISessionFactory sessionFactory, IIssueClient issueClient, ICommentClient commentClient) : base(sessionFactory)
        {
            this.issueClient = issueClient;
            this.commentClient = commentClient;
        }

        public void Index()
        {
            IQueryable<Project> projects = session.Query<Project>();

            foreach (Project project in projects)
            {
                using (ITransaction trans = session.BeginTransaction())
                {
                    List<Issue> items = issueClient.List(project.Repository, project.MilestoneId);

                    foreach (Issue issue in items)
                    {
                        var newIssue = new Model.Issue
                                           {
                                               Body = issue.Body,
                                               Devvers = issue.Devvers,
                                               Hours = issue.Hours,
                                               Name = issue.Name,
                                               Number = issue.Number,
                                               Project = project,
                                           };

                        newIssue.ChangeStates.Add(new ChangeState
                                                      {CreatedAt = issue.CreatedAt, IssueState = IssueState.Open});
                        newIssue.Changes.Add(new Change {CreatedAt = issue.UpdatedAt});
                        if (issue.ClosedAt.HasValue)
                            newIssue.ChangeStates.Add(new ChangeState
                                                          {
                                                              CreatedAt = issue.ClosedAt.Value,
                                                              IssueState = IssueState.Closed
                                                          });

                        foreach (Label label in issue.Labels)
                        {
                            Model.Label gitleLabel =
                                session.Query<Model.Label>().FirstOrDefault(
                                    x => x.Name == label.Name && x.Project == project) ??
                                new Model.Label {Name = label.Name, Color = label.Color, Project = project};
                            newIssue.Labels.Add(gitleLabel);
                        }

                        List<Comment> comments = commentClient.List(project.Repository, issue.Number).ToList();
                        foreach (Comment comment in comments)
                        {
                            User user = session.Query<User>().FirstOrDefault(x => x.FullName == comment.Name);
                            var newComment = new Model.Comment
                                                 {
                                                     CreatedAt = comment.CreatedAt,
                                                     Issue = newIssue,
                                                     Text = comment.Text,
                                                     User = user
                                                 };
                            newIssue.Comments.Add(newComment);
                        }

                        session.Save(newIssue);
                    }
                    trans.Commit();
                }
            }

            RenderText("import complete");
        }
    }
}