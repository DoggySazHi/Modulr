using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Modulr.Tester;
using Newtonsoft.Json.Linq;

namespace Modulr.Controllers.Auth
{
    public class Capcha
    {
        private readonly HttpClient _client;
        private readonly ModulrConfig _config;
        
        public Capcha(HttpClient client, ModulrConfig config)
        {
            _client = client;
            _config = config;
        }

        public async Task<bool> VerifyCapcha(string token)
        {
            var parameters = new Dictionary<string, string>
            {
                { "secret", _config.reCAPCHASecretKey },
                { "response", token }
            };

            var request = new FormUrlEncodedContent(parameters);
            var response = await _client.PostAsync("https://www.google.com/recaptcha/api/siteverify", request);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseJSON = JObject.Parse(responseString)["success"];
            return responseJSON?.ToObject<bool>() ?? false;
        }
    }
}