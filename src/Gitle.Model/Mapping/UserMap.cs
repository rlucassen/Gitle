namespace Gitle.Model.Mapping
{
    #region Usings

    using Nested;

    #endregion

    public class UserMap : ModelBaseMap<User>
    {
        public UserMap()
        {
            Component(x => (Password) x.Password).ColumnPrefix("Password_");
            Map(x => x.Name);
            Map(x => x.EmailAddress);
            Map(x => x.IsAdmin);
            HasManyToMany(x => x.Projects).Table("User_Project").Cascade.All();
        }
    }
}