namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum InvoiceState
    {
        Unknown = 0,
        [Description("concept")]
        Concept = 1,
        [Description("definitief")]
        Definitive = 2,
        [Description("gearchiveerd")]
        Archived = 3,
    }
}