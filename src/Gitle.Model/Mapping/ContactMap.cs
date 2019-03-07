namespace Gitle.Model.Mapping
{
    public class ContactMap : ModelBaseMap<Contact>
    {
        public ContactMap()
        {
            Map(x => x.FullName);
            Map(x => x.Email);
            Map(x => x.PhoneNumber);
        }
    }
}