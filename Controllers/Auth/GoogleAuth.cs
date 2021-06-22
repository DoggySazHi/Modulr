using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers.Auth
{
    public class GoogleAuth
    {
        private readonly ModulrConfig _config;
        private readonly SqlQuery _query;
        
        public GoogleAuth(ModulrConfig config, SqlQuery query)
        {
            _config = config;
            _query = query;
        }
        
        public enum LoginStatus
        {
            Invalid,
            BadAudience,
            BadIssuer,
            ExpiredToken,
            BadDomain,
            Banned,
            Success
        }
        
        public async Task<(LoginStatus Status, GoogleJsonWebSignature.Payload User)> Verify(string token)
        {
            if (token == null)
                return (LoginStatus.Invalid, null);

            GoogleJsonWebSignature.Payload validation;
            try
            {
                validation = await GoogleJsonWebSignature.ValidateAsync(token);
            }
            catch (Exception) // Really bad programming practice, but that's all that is necessary. It's invalid.
            {
                validation = null;
            }

            if (validation == null)
                return (LoginStatus.Invalid, null);
            if (!validation.AudienceAsList.Contains(_config.GoogleClientKey))
                return (LoginStatus.BadAudience, null);
            if (validation.Issuer != "accounts.google.com" && validation.Issuer != "https://accounts.google.com")
                return (LoginStatus.BadIssuer, null);
            if (validation.ExpirationTimeSeconds == null ||
                validation.ExpirationTimeSeconds < DateTimeOffset.Now.ToUnixTimeSeconds())
                return (LoginStatus.ExpiredToken, null);
            if (!string.IsNullOrEmpty(_config.HostedDomain) && validation.HostedDomain != _config.HostedDomain)
                return (LoginStatus.BadDomain, null);
            
            await _query.Register(validation.Subject, validation.Name, validation.Email);
            
            var banStatus = await _query.GetRole(validation.Subject);
            if (banStatus.HasFlag(Role.Banned))
                return (LoginStatus.Banned, null);
            
            return (LoginStatus.Success, validation);
        }
    }
}