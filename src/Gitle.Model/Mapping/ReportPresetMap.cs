namespace Gitle.Model.Mapping
{
    public class ReportPresetMap : ModelBaseMap<ReportPreset>
    {
         public ReportPresetMap()
         {
             Map(x => x.Name);
             Map(x => x.ReportString);
             References(x => x.User);
         }
    }
}