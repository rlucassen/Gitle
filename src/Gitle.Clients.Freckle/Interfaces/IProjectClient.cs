namespace Gitle.Clients.Freckle.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IProjectClient
    {
        List<Project> List();
        Project Show(int id);
    }
}