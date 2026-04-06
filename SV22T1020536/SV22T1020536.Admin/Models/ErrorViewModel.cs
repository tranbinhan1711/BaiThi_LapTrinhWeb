namespace SV22T1020536.Models;

/// <summary>
/// Thông tin RequestId hiển thị trên trang lỗi.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
