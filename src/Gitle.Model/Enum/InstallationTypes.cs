namespace Gitle.Model.Enum
{
    using System.ComponentModel;

    public enum InstallationTypes
    {
        Unknown = 0,
        [Description("Live")]
        Live = 10,
        [Description("Acceptatie")]
        Acceptance = 20,
        [Description("Demo")]
        Demo = 30,
    }
}