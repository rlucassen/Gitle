namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum InvoiceState
    {
        Unknown = 0,
        [Description("Concept")]
        Concept = 1,
        [Description("Definitief")]
        Definitive = 2,
        [Description("Gearchiveerd")]
        Archived = 3,
    }
}