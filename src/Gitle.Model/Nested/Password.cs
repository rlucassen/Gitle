namespace Gitle.Model.Nested
{
    #region Usings

    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using Gitle.Model.Interfaces.Model;
    using SimpleCrypto;

    #endregion

    public class Password : IPassword
    {
        public Password(string password)
            : this()
        {
            EncryptPassword(password);
        }

        public Password()
        {
            var password = GeneratePassword();
            EncryptPassword(password);
        }

        public virtual string Salt { get; protected set; }

        public virtual string EncriptedPassword { get; protected set; }

        public virtual string Hash { get; protected set; }

        public virtual bool Match(string password)
        {
            var cryptoService = new PBKDF2();
            return String.CompareOrdinal(cryptoService.Compute(password, Salt), EncriptedPassword) == 0;
        }

        /// <summary>
        ///     Salt and encript the given password.
        /// </summary>
        /// <param name="password"> </param>
        private void EncryptPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException("password");

            var cryptoService = new PBKDF2();

            EncriptedPassword = cryptoService.Compute(password);
            Salt = cryptoService.Salt;
        }

        public static string GeneratePassword(int lenght = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz@#$%^&*!";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, lenght)
                          .Select(c => c[random.Next(c.Length)])
                          .ToArray());
        }

        public void GenerateHash()
        {
            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            Hash = BitConverter.ToString(bytes).Replace("-", "");
        }

    }
}