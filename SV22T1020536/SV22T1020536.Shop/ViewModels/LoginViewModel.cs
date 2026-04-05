using System.ComponentModel.DataAnnotations;

namespace SV22T1020536.Shop.ViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;
}
