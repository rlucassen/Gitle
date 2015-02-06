namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum IssueState
    {
        Unknown = 0,
        [Description("geopend")]
        Open = 1,
        [Description("gesloten")]
        Closed = 2,
    }
}