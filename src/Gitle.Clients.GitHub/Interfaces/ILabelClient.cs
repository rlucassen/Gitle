namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface ILabelClient
    {
        List<Label> List(string repo);
        Label Get(string repo, int labelId);
        Label Post(string repo, Label label);
    }
}