namespace Gitle.Clients.GitHub.Interfaces
{
    using Models;

    public interface IOAuthClient
    {
        AccessToken Login(string code);
    }
}