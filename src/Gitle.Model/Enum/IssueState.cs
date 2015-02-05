namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum IssueState
    {
        [Description("geopent")]
        Open = 0,
        [Description("gesloten")]
        Closed = 1,
    }
}