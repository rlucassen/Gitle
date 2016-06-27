namespace Gitle.Web.Helpers
{
    using System;
    using Model;

    public class ProjectClosedException : Exception
    {
        public ProjectClosedException(Project project) : base($"Project \"{project.Name}\" is gesloten")
        {
        }
    }
}