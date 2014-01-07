namespace Gitle.Clients.GitHub.Interfaces
{
    using System.Collections.Generic;
    using Models;

    public interface IUserClient
    {
        User Get(string accessToken);
    }
}