using System.ComponentModel.DataAnnotations;

namespace WebAdvert.Web.Models.Accounts
{
    public class LoginViewModel
    {
        [Required] [EmailAddress] public string Email { get; set; }

        [Required] public string Password { get; set; }

        [Required] public bool RememberMe { get; set; }
    }
}