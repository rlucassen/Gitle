namespace Gitle.Model.Interfaces.Model
{
    #region Usings

    using System;

    #endregion

    public interface IPassword
    {
        bool Match(string password);

        string Salt { get; }
        string EncriptedPassword { get; }
        DateTime CreationDate { get; }
        bool Temporary { get; }
    }
}