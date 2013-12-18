namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using Admin;
    using Clients.GitHub.Interfaces;
    using Clients.GitHub.Models;
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
            var project = repository.FindBySlug(projectSlug);
            PropertyBag.Add("item", project);
            if (fullrepo == "0")
            {
                PropertyBag.Add("items", new List<Milestone>());
            }
            else
            {
                PropertyBag.Add("items", client.List(fullrepo));
            }

            CancelLayout();
        }
    }
}