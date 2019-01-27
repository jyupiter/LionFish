using Jint;
using LionFishWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace LionFishWeb.Utility
{
    public static class Constants
    {
        public static string Conn => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public const string captchaSecret = "6LcPQ30UAAAAABFqqSAazpaMGWObvP6lCuSZggbR";

        private static readonly int SaltLengthLimit = 16;
        private static byte[] GetSalt()
        {
            return GetSalt(SaltLengthLimit);
        }
        private static byte[] GetSalt(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }
            return salt;
        }
        public static string SaltPass(string pass)
        {
            byte[] salt = GetSalt();
            var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, 250);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hbyt = new byte[36];
            Array.Copy(salt, 0, hbyt, 0, 16);
            Array.Copy(hash, 0, hbyt, 16, 20);
            return Convert.ToBase64String(hbyt);
        }
        public static bool AuthUser(User user, string test)
        {
            try
            {
                byte[] hbyt = Convert.FromBase64String(user.PasswordHash);
                byte[] salt = new byte[16];
                Array.Copy(hbyt, 0, salt, 0, 16);
                var pbkdf2 = new Rfc2898DeriveBytes(test, salt, 250);
                byte[] hash = pbkdf2.GetBytes(20);

                for (int i = 0; i < 20; i++)
                    if (hbyt[i + 16] != hash[i])
                        return false;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static int ZXCVBN(string p)
        {
            string startupPath = HttpRuntime.AppDomainAppPath;
            Debug.WriteLine(HttpRuntime.AppDomainAppPath);

            var s = System.IO.File.ReadAllText(startupPath + "/Scripts/zxcvbn.js");
            int x = 10;

            var engine = new Engine();
            var al = new System.Collections.ArrayList(2)
            {
                x,
                p
            };
            engine.SetValue("al", al);
            engine.Execute(s);
            engine.Execute(@"
                var z = zxcvbn(al[1]);
                al[0] = z.score;
            ");

            return Convert.ToInt16(al[0]);
        }
    }
}