namespace Gitle.Model.Mapping
{
    public class CustomerMap : ModelBaseMap<Customer>
    {
         public CustomerMap()
         {
             Map(x => x.Name);
         }
    }
}