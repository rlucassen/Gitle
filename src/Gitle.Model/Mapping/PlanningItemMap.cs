namespace Gitle.Model.Mapping
{
    public class PlanningItemMap : ModelBaseMap<PlanningItem>
    {
        public PlanningItemMap()
        {
            Map(x => x.Start);
            Map(x => x.End).Column("[End]");
            References(x => x.User);
            Map(x => x.Resource);
            Map(x => x.Text);
        }
    }
}