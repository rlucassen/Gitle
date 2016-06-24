namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum ProjectType
    {
        Unknown = 0,
        [Description("Initieel project")]
        Initial = 1,
        [Description("Serviceproject")]
        Service = 2,
        [Description("Intern project")]
        Internal = 3,
        [Description("Administratief project")]
        Administration = 4,
    }
}