namespace Gitle.Model.Mapping
{
    public class SettingMap : ModelBaseMap<Setting>
    {
        public SettingMap()
        {
            Map(x => x.ClosedForBookingsBefore);
        }
    }
}