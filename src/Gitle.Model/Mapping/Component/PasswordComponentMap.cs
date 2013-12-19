namespace Gitle.Model.Mapping.Component
{
    #region Usings

    using FluentNHibernate.Mapping;
    using Nested;

    #endregion

    public class PasswordComponentMap : ComponentMap<Password>
    {
        public PasswordComponentMap()
        {
            Map(x => x.Salt);
            Map(x => x.EncriptedPassword);
            Map(x => x.Hash);
        }
    }
}