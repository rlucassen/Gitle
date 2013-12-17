namespace Gitle.Model.Mapping
{
    #region Usings

    using FluentNHibernate.Mapping;

    #endregion

    public abstract class ModelBaseMap<T> : ClassMap<T> where T : ModelBase
    {
        public ModelBaseMap()
        {
            Id(x => x.Id);
            Map(x => x.Guid);
            Map(x => x.IsActive);
        }
    }
}