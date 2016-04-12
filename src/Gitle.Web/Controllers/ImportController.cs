namespace Gitle.Web.Controllers
{
    using System.Linq;
    using Clients.GitHub.Interfaces;
    using Model;
    using Model.Enum;
    using NHibernate;
    using NHibernate.Linq;

    public class ImportController : SecureController
    {
        private readonly ICommentClient commentClient;
        private readonly IIssueClient issueClient;
        private readonly ISession session;

        public ImportController(ISessionFactory sessionFactory, IIssueClient issueClient, ICommentClient commentClient)
        {
            this.issueClient = issueClient;
            this.commentClient = commentClient;
            session = sessionFactory.GetCurrentSession();
        }

        public void Index()
        {
            var projects = session.Query<Project>();

            foreach (var project in projects)
            {
                using (var trans = session.BeginTransaction())
                {
                    var items = issueClient.List(project.Repository, project.MilestoneId);

                    foreach (var issue in items)
                    {
                        var newIssue = new Issue
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

                        foreach (var label in issue.Labels)
                        {
                            var gitleLabel =
                                session.Query<Label>().FirstOrDefault(
                                    x => x.Name == label.Name && x.Project == project) ??
                                new Label {Name = label.Name, Color = label.Color, Project = project};
                            newIssue.Labels.Add(gitleLabel);
                        }

                        var comments = commentClient.List(project.Repository, issue.Number).ToList();
                        foreach (var comment in comments)
                        {
                            var user = session.Query<User>().FirstOrDefault(x => x.FullName == comment.Name);
                            var newComment = new Comment
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