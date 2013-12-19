namespace Gitle.Model.Interfaces.Model
{
    #region Usings

    using System;

    #endregion

    public interface IPassword
    {
        bool Match(string password);
        void GenerateHash();

        string Salt { get; }
        string EncriptedPassword { get; }
        string Hash { get; }
    }
}