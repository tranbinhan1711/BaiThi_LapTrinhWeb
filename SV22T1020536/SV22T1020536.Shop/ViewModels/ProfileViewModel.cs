using System.ComponentModel.DataAnnotations;

namespace SV22T1020536.Shop.ViewModels;

public class ProfileViewModel
{
    public int CustomerID { get; set; }

    [Required]
    [Display(Name = "Tên khách hàng")]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Người liên hệ")]
    public string ContactName { get; set; } = string.Empty;

    [Display(Name = "Tỉnh/Thành")]
    public string? Province { get; set; }
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }
    [Display(Name = "Điện thoại")]
    public string? Phone { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
