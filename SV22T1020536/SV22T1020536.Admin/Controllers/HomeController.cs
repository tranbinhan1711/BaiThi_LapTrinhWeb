using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020536.Models;

namespace SV22T1020536.Admin.Controllers;

    [Authorize]
public class HomeController : Controller
{
    /// <summary>
    /// Giao diá»‡n trang chá»§ cá»§a há»‡ thá»‘ng
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        return View();
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
