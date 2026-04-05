using System.ComponentModel.DataAnnotations;

namespace SV22T1020536.Shop.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Họ và tên")]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
