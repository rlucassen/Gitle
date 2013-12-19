namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using Admin;
    using Clients.GitHub.Interfaces;
    using Clients.GitHub.Models;
    using Model;
    using Model.Interfaces.Repository;
    using Helpers;

    public class MilestoneController : SecureController
    {
        private IProjectRepository repository;
        private IMilestoneClient client;

        public MilestoneController(IMilestoneClient client, IProjectRepository repository)
        {
            this.repository = repository;
            this.client = client;
        }

        [Admin]
        public void Select(string projectSlug, string fullrepo)
        {
            var project = string.IsNullOrEmpty(projectSlug) ? new Project() : repository.FindBySlug(projectSlug);
            if (fullrepo == "0")
            {
                PropertyBag.Add("items", new List<Milestone>());
            }
            else
            {
                project.Repository = fullrepo;
                PropertyBag.Add("items", client.List(fullrepo));
            }
            PropertyBag.Add("item", project);
            CancelLayout();
        }
    }
}