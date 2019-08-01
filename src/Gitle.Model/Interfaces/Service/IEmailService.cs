namespace Gitle.Model.Interfaces.Service
{
    using Model;

    public interface IEmailService
    {
        void SendPasswordLink(User user);
        void SendIssueActionNotification(IIssueAction action);
        void SendHandOverNotification(HandOver handOver);
    }
}