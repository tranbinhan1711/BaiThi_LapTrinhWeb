using System.ComponentModel.DataAnnotations;

namespace SV22T1020536.Admin.ViewModels
{
    /// <summary>
    /// Dữ liệu form đăng nhập quản trị.
    /// </summary>
    public class AdminLoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Form đổi mật khẩu cho nhân viên đang đăng nhập.
    /// </summary>
    public class AdminChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

