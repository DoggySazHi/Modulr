using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Modulr.Tester;

namespace Modulr.Controllers
{
    public class GoogleAuth
    {
        private readonly ModulrConfig _config;
        private readonly MySqlQuery _query;
        
        public GoogleAuth(ModulrConfig config, MySqlQuery query)
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
            Success
        }
        
        public async Task<LoginStatus> Verify(string token)
        {
            if (token == null)
                return LoginStatus.Invalid;

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
                return LoginStatus.Invalid;
            if (!validation.AudienceAsList.Contains(_config.GoogleClientKey))
                return LoginStatus.BadAudience;
            if (validation.Issuer != "accounts.google.com" && validation.Issuer != "https://accounts.google.com")
                return LoginStatus.BadIssuer;
            if (validation.ExpirationTimeSeconds == null ||
                validation.ExpirationTimeSeconds < DateTimeOffset.Now.ToUnixTimeSeconds())
                return LoginStatus.ExpiredToken;
            if (!string.IsNullOrEmpty(_config.HostedDomain) && validation.HostedDomain != _config.HostedDomain)
                return LoginStatus.BadDomain;
            await _query.Register(validation.Subject, validation.Name, validation.Email);
            return LoginStatus.Success;
        }
    }
}