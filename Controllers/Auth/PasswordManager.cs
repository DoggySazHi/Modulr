using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Modulr.Models;

namespace Modulr.Controllers.Auth
{
    public class PasswordManager
    {
        private readonly SqlQuery _query;
        
        public PasswordManager(SqlQuery query)
        {
            _query = query;
        }

        public async Task SetPassword(int user, string password)
        {
            var hash = new Rfc2898DeriveBytes(password, 32, 10000);
            var hashBytes = hash.GetBytes(64);
            var saltBytes = hash.Salt;
            var hashString = Convert.ToBase64String(hashBytes);
            var saltString = Convert.ToBase64String(saltBytes);

            await _query.UpdateUserLogin(user, new UserLogin
            {
                Password = hashString,
                Salt = saltString
            });
        }
        
        public async Task<string> Login(int id, string password)
        {
            var user = await _query.GetUserLogin(id);
            if (!VerifyPassword(user, password))
                return null;
            
            return await GenerateCookie(id);
        }

        public async Task<string> GenerateCookie(int id)
        {
            var loginKey = GetAuthCode();
            await _query.UpdateUserLogin(id, new UserLogin
            {
                LoginCookie = loginKey,
                LoginExpiration = DateTimeOffset.Now.AddDays(14)
            });

            return loginKey;
        }
        
        /// <summary>
        /// Determine whether the user's cookie is valid.
        /// </summary>
        /// <param name="id">The user's Modulr ID.</param>
        /// <param name="cookie">The user's cookie.</param>
        /// <returns>Whether their cookie is still valid or not.</returns>
        public async Task<bool> VerifySession(int id, string cookie)
        {
            var user = await _query.GetUserLogin(id);
            if (user == null)
                return false;
            return user.LoginExpiration >= DateTimeOffset.Now && SlowEquals(cookie, user.LoginCookie);
        }

        public async Task<bool> ChangePassword(int id, string oldPassword, string newPassword)
        {
            var user = await _query.GetUserLogin(id);
            if (!VerifyPassword(user, oldPassword))
                return false;
            await SetPassword(id, newPassword);
            return true;
        }
        
        private bool VerifyPassword(UserLogin user, string password)
        {
            if (user.Password == null) // If they're using Google logins, we don't want them to login conventionally.
                return false;

            var hash = new Rfc2898DeriveBytes(password, Convert.FromBase64String(user.Salt), 10000);
            var hashBytes = hash.GetBytes(64);
            var sourceBytes = Convert.FromBase64String(user.Password);

            return SlowEquals(hashBytes, sourceBytes);
        }
        
        private static string GetAuthCode()
        {
            var data = new byte[64];
            using(var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            return Convert.ToBase64String(data);
        }

        private static bool SlowEquals(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
        {
            // In C++ I always get warnings for bit-wise ops on signed values... why not C#?
            var diff = (uint) a.Count ^ (uint) b.Count;
            for (var i = 0; i < a.Count && i < b.Count; i++)
                diff |= (uint) a[i] ^ b[i];
            return diff == 0;
        }

        // Note how I use ASCII encoding; this is because I can't always trust the user to put Base64 text (especially for cookies!)
        private static bool SlowEquals(string a, string b) =>
            SlowEquals(Encoding.ASCII.GetBytes(a), Encoding.ASCII.GetBytes(b));
    }
}