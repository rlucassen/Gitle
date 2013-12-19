namespace Gitle.Web.Controllers
{
    using Castle.MonoRail.Framework;
    using Clients.GitHub.Models;
    using Clients.GitHub.Models.Hooks;
    using Model.Interfaces.Service;
    using Newtonsoft.Json;

    public class GitHubHookController : SmartDispatcherController
    {
        private readonly IEmailService emailService;

        public GitHubHookController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        public void Hook(string payload)
        {
            var hook = JsonConvert.DeserializeObject<HookPayload>(payload);
            emailService.SendHookNotification(hook);
            RenderText("done");
        }

    }
}