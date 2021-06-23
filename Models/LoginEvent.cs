using System.ComponentModel.DataAnnotations;

namespace Modulr.Models
{
    public class LoginEvent
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public bool IsLikelyValid()
        {
            if (Email == null || Password == null)
                return false;
            return new EmailAddressAttribute().IsValid(Email);
        }
    }
}