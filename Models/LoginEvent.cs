using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Modulr.Controllers.Auth;

namespace Modulr.Models;

public class LoginEvent
{
    public string CaptchaToken { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public async Task<bool> IsLikelyValid(Captcha captcha)
    {
        if (Email == null || Password == null)
            return false;
        if (!new EmailAddressAttribute().IsValid(Email))
            return false;
        return await captcha.VerifyCaptcha(CaptchaToken);
    }
}