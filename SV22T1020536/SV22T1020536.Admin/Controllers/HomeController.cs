using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020536.Models;

namespace SV22T1020536.Admin.Controllers;

/// <summary>
/// Trang chủ và lỗi chung của ứng dụng quản trị.
/// </summary>
[Authorize]
public class HomeController : Controller
{
    /// <summary>
    /// Trang chủ bảng điều khiển sau khi đăng nhập.
    /// </summary>
    /// <returns>View trang chủ.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Trang chính sách quyền riêng tư (mẫu).
    /// </summary>
    /// <returns>View Privacy.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Trang lỗi với RequestId để tra cứu log.
    /// </summary>
    /// <returns>View Error.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
