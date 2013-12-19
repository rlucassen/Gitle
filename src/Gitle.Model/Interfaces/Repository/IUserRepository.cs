namespace Gitle.Model.Interfaces.Repository
{
    using System.Collections.Generic;

    public interface IUserRepository : IBaseRepository<User>
    {
        User FindByName(string name);
        IList<User> FindByEmail(string email);
        IList<User> FindByPasswordHash(string hash);
    }
}