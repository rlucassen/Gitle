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
            Map(x => x.Salutation);
            Map(x => x.Name);
            Map(x => x.FullName);
            Map(x => x.EmailAddress);
            Map(x => x.IsAdmin);
            Map(x => x.IsDanielle);
            Map(x => x.CanBookHours);
            Map(x => x.Phone);
            Map(x => x.Mobile);
            Map(x => x.Position);
            Map(x => x.Company);
            Map(x => x.GitHubUsername);
            Map(x => x.GitHubAccessToken);
            Map(x => x.FreckleEmail);
            Map(x => x.DefaultState);
            Map(x => x.Comments).CustomSqlType("nvarchar(max)");
            Map(x => x.Color);
            Map(x => x.JamesEmployeeId);
            HasMany(x => x.Projects).Inverse().Cascade.AllDeleteOrphan();
            HasMany(x => x.FilterPresets);
            References(x => x.Customer);

        }
    }
}