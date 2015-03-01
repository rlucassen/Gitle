namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class CorrectionMap : ModelBaseMap<Correction>
    {
        public CorrectionMap()
        {
            Map(x => x.Description);
            Map(x => x.Price);

            References(x => x.Invoice).Column("Invoice_id");
        }
    }
}