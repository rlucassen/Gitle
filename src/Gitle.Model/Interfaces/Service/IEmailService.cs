namespace Gitle.Model.Interfaces.Service
{
    public interface IEmailService
    {
        void SendPasswordLink(User user);
    }
}