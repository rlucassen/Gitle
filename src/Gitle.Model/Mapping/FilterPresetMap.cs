namespace Gitle.Model.Mapping
{
    public class FilterPresetMap : ModelBaseMap<FilterPreset>
    {
         public FilterPresetMap()
         {
             Map(x => x.Name);
             Map(x => x.FilterString);
             References(x => x.User);
         }
    }
}