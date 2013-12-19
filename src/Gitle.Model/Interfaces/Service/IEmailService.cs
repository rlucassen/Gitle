namespace Gitle.Model.Interfaces.Service
{
    using Clients.GitHub.Models.Hooks;

    public interface IEmailService
    {
        void SendHookNotification(HookPayload hookPayload);
        void SendPasswordLink(User user);
    }
}