namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum Salutation
    {
        [Description("Onbekend")]
        Unknown = 0,
        [Description("De heer")]
        Male = 10,
        [Description("Mevrouw")]
        Female = 20,
    }
}