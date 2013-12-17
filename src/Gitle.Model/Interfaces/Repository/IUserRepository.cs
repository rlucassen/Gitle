namespace Gitle.Model.Interfaces.Repository
{
    public interface IUserRepository
    {
        User FindByName(string name);
    }
}