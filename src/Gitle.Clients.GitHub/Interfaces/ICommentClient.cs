namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface ICommentClient
    {
        List<Comment> List(string repo, int issueId);
        Comment Post(string repo, int issueId, Comment comment);
    }
}