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

        /// <summary>
        /// Set the password for a user.
        /// For security reasons, this should only be used for registration; admins should generate a random password.
        /// </summary>
        /// <param name="user">The Modulr ID of the user.</param>
        /// <param name="password">The new password for the user.</param>
        public async Task SetPassword(int user, string password)
        {
            var hash = new Rfc2898DeriveBytes(password, 32, 10000);
            var hashBytes = hash.GetBytes(48);
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
            await _query.UpdateUserCookie(id, new UserLogin
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

        /// <summary>
        /// Allow a user to change their password, assuming they have their old password (or a temporary one).
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        /// <param name="oldPassword">The previous password of the user.</param>
        /// <param name="newPassword">The new password of the user.</param>
        /// <returns>Whether the operation was successful.</returns>
        public async Task<bool> ChangePassword(int id, string oldPassword, string newPassword)
        {
            var user = await _query.GetUserLogin(id);
            if (!VerifyPassword(user, oldPassword))
                return false;
            await SetPassword(id, newPassword);
            return true;
        }
        
        /// <summary>
        /// Generate a temporary password for the user.
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        /// <returns>The new temporary password.</returns>
        public async Task<string> RandomPassword(int id)
        {
            var password = GeneratePassword(16);
            await SetPassword(id, password);
            return password;
        }
        
        private static readonly char[] RandomPasswordChars = 
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

        private static string GeneratePassword(int size)
        {            
            var data = new byte[4 * size];
            
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }

            var output = new char[size];
            
            for (var i = 0; i < size; i++)
            {
                var rng = BitConverter.ToUInt32(data, i * 4);
                var rngChar = rng % output.Length;
                output[i] = RandomPasswordChars[rngChar];
            }

            return new string(output);
        }
        
        private bool VerifyPassword(UserLogin user, string password)
        {
            if (user.Password == null) // If they're using Google logins, we don't want them to login conventionally.
                return false;

            var hash = new Rfc2898DeriveBytes(password, Convert.FromBase64String(user.Salt), 10000);
            var hashBytes = hash.GetBytes(48);
            var sourceBytes = Convert.FromBase64String(user.Password);

            return SlowEquals(hashBytes, sourceBytes);
        }
        
        /// <summary>
        /// Generate a random string representing 48 bytes of random things (should be 64 char in length).
        /// This is used to verify the user's cookie with the database.
        /// </summary>
        /// <returns>A random string.</returns>
        private static string GetAuthCode()
        {
            var data = new byte[48];
            using(var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// A slow-equals algorithm that XORs values to compare hashes.
        /// Note that this method is redundant, as PBKDF2 is not vulnerable to timed comparison attacks.
        /// </summary>
        /// <param name="a">The first hash to compare.</param>
        /// <param name="b">The second hash to compare.</param>
        /// <returns></returns>
        private static bool SlowEquals(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
        {
            // In C++ I always get warnings for bit-wise ops on signed values... why not C#?
            var diff = (uint) a.Count ^ (uint) b.Count;
            for (var i = 0; i < a.Count && i < b.Count; i++)
                diff |= (uint) a[i] ^ b[i];
            return diff == 0;
        }
        
        /// <summary>
        /// Slow-equals, but for strings.
        /// Note how I use ASCII encoding; this is because I can't always trust the user to put Base64 text (especially for cookies!)
        /// </summary>
        /// <param name="a">The first string to compare.</param>
        /// <param name="b">The second string to compare.</param>
        /// <returns></returns>
        private static bool SlowEquals(string a, string b) =>
            SlowEquals(Encoding.ASCII.GetBytes(a), Encoding.ASCII.GetBytes(b));
    }
}