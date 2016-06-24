namespace Gitle.Model.Mapping
{
    public class PlanningItemMap : ModelBaseMap<PlanningItem>
    {
        public PlanningItemMap()
        {
            Map(x => x.Start);
            Map(x => x.End);
            References(x => x.User);
            References(x => x.Project);
        }
    }
}