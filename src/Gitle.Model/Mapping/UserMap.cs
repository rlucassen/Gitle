﻿namespace Gitle.Model.Mapping
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
            Map(x => x.FullName);
            Map(x => x.EmailAddress);
            Map(x => x.IsAdmin);
            Map(x => x.Phone);
            Map(x => x.GitHubUsername);
            HasMany(x => x.Projects).Cascade.AllDeleteOrphan();
        }
    }
}