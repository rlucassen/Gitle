namespace Gitle.Model.Helpers
{
    using System;
    using System.Security.Cryptography;

    public static class HashHelper
    {
         public static string GenerateHash()
         {
             var bytes = new byte[16];
             using (var rng = new RNGCryptoServiceProvider())
             {
                 rng.GetBytes(bytes);
             }

             return BitConverter.ToString(bytes).Replace("-", "");
         }
    }
}