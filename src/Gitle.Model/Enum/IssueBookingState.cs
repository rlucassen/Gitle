namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum IssueBookingState
    {
        Unknown = 0,
        [Description("geboekt")]
        Any = 1,
        [Description("ongeboekt")]
        None = 2
    }
}