namespace Gitle.Model.Mapping
{
    public class DocumentMap : ModelBaseMap<Document>
    {
        public DocumentMap ()
        {
            Map(x => x.Name);
            Map(x => x.Path);
            Map(x => x.DateUploaded);
            References(x => x.User);
        }
         
    }
}