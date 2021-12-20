using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Modulr.Controllers.Auth;

namespace Modulr.Models;

public class RegisterUser
{
    public string CaptchaToken { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
        
    public async Task<bool> IsLikelyValid(Captcha capcha)
    {
        if (CaptchaToken == null || Name == null || Email == null || Password == null)
            return false;
        if (!await capcha.VerifyCaptcha(CaptchaToken))
            return false;
        if (!new EmailAddressAttribute().IsValid(Email))
            return false;
        if (Name.Length < 3 || Password.Length < 5)
            return false;

        return true;
    }
}