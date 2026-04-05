using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SV22T1020536.Admin.ViewModels;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.HR;
using System.Security.Claims;

namespace SV22T1020536.Admin.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Giao diá»‡n Ä‘Äƒng nháº­p vÃ o há»‡ thá»‘ng
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdminLoginViewModel());
        }
        /// <summary>
        /// Xá»­ lÃ½ Ä‘Äƒng nháº­p
        /// </summary>
        /// <param name="email">Äá»‹a chá»‰ email Ä‘Äƒng nháº­p cá»§a ngÆ°á»i dÃ¹ng</param>
        /// <param name="password">Máº­t kháº©u Ä‘Äƒng nháº­p</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var email = model.Email?.Trim() ?? "";
            var employee = await HRDataService.GetEmployeeByEmailAsync(email);
            if (employee == null || !employee.IsWorking || string.IsNullOrWhiteSpace(employee.Password) || employee.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
                new Claim(ClaimTypes.Name, employee.FullName ?? employee.Email),
                new Claim("roleNames", employee.RoleNames ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// ÄÄƒng xuáº¥t khá»i há»‡ thá»‘ng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new AdminRegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AdminRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();
            if (!await HRDataService.ValidateEmployeeEmailAsync(email, 0))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
                return View(model);
            }

            var roleNames = model.RoleNames?.Trim() ?? "";
            // Chỉ cho phép các quyền trong UI đăng ký.
            if (roleNames != "Staff" && roleNames != "Manager" && roleNames != "Admin")
            {
                ModelState.AddModelError(nameof(model.RoleNames), "Chức vụ không hợp lệ.");
                return View(model);
            }

            var employee = new Employee
            {
                FullName = model.FullName.Trim(),
                Email = email,
                Password = model.Password,
                IsWorking = true,
                RoleNames = roleNames,
            };

            await HRDataService.AddEmployeeAsync(employee);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Giao diá»‡n hiá»ƒn thá»‹ thÃ´ng tin cÃ¡ nhÃ¢n
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        /// <summary>
        /// Cáº­p nháº­t thÃ´ng tin cÃ¡ nhÃ¢n
        /// </summary>
        /// <param name="fullName">Há» vÃ  tÃªn cáº§n cáº­p nháº­t</param>
        /// <param name="email">Äá»‹a chá»‰ email cáº§n cáº­p nháº­t</param>
        /// <param name="phone">Sá»‘ Ä‘iá»‡n thoáº¡i cáº§n cáº­p nháº­t</param>
        /// <param name="address">Äá»‹a chá»‰ cáº§n cáº­p nháº­t</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Profile(string fullName, string email, string phone, string address)
        {
            // Xá»­ lÃ½ cáº­p nháº­t profile
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Giao diá»‡n thay Ä‘á»•i máº­t kháº©u
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Xá»­ lÃ½ thay Ä‘á»•i máº­t kháº©u
        /// </summary>
        /// <param name="oldPassword">Máº­t kháº©u hiá»‡n táº¡i cáº§n kiá»ƒm tra</param>
        /// <param name="newPassword">Máº­t kháº©u má»›i cáº§n thiáº¿t láº­p</param>
        /// <param name="confirmPassword">XÃ¡c nháº­n máº­t kháº©u má»›i cáº§n thiáº¿t láº­p (pháº£i trÃ¹ng vá»›i newPassword)</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // Kiá»ƒm tra vÃ  Ä‘á»•i máº­t kháº©u (giáº£ láº­p)
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Máº­t kháº©u xÃ¡c nháº­n khÃ´ng trÃ¹ng khá»›p.");
                return View();
            }
            
            // Xá»­ lÃ½ Ä‘á»•i máº­t kháº©u thÃ nh cÃ´ng...
            return RedirectToAction("Index", "Home");
        }
    }
}
