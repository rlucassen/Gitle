namespace Gitle.Model.Interfaces.Repository
{
    using System.Collections.Generic;

    public interface IUserRepository : IBaseRepository<User>
    {
        User FindByName(string name);
        User FindByFullName(string fullName);
        IList<User> FindByEmail(string email);
        IList<User> FindByPasswordHash(string hash);
        IList<User> FindByGithubUser(string githubUser);
    }
}