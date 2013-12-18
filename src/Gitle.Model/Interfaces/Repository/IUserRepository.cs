namespace Gitle.Model.Interfaces.Repository
{
    public interface IUserRepository : IBaseRepository<User>
    {
        User FindByName(string name);
    }
}