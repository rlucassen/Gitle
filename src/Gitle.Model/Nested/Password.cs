namespace Gitle.Model.Nested
{
    #region Usings

    using System;
    using System.Linq;
    using Gitle.Model.Interfaces.Model;
    using SimpleCrypto;

    #endregion

    public class Password : IPassword
    {
        protected Password()
        {
            CreationDate = DateTime.Now;
        }

        public Password(string password, bool temporary = false)
            : this()
        {
            Temporary = temporary;
            EncryptPassword(password);
        }

        public virtual string Salt { get; protected set; }

        public virtual string EncriptedPassword { get; protected set; }

        public virtual DateTime CreationDate { get; private set; }

        public virtual bool Temporary { get; protected set; }

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

        public class NullPassword : IPassword
        {
            public bool Match(string password)
            {
                return false;
            }

            public string Salt
            {
                get { return string.Empty; }
            }

            public string EncriptedPassword
            {
                get { return string.Empty; }
            }

            public DateTime CreationDate
            {
                get { return DateTime.MinValue; }
            }

            public bool Temporary
            {
                get { return true; }
            }
        }
    }
}