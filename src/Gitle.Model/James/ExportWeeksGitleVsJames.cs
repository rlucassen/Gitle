namespace Gitle.Model.James
{
    public class ExportWeeksGitleVsJames
    {
        public string NameOfEmployee { get; set; }
        public ExportWeek[] Weeks { get; set; } = new ExportWeek[53];
    }

    public struct ExportWeek
    {
        public double MinutesGitle { get; set; }
        public double MinutesJames { get; set; }
    }
}