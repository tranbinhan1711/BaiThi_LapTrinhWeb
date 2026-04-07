using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020536.Admin.AppCodes;
using SV22T1020536.Admin.ViewModels;
using SV22T1020536.BusinessLayers;
using System.Security.Claims;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// Xác thực tài khoản nhân viên: đăng nhập, hồ sơ và mật khẩu.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Hiển thị form đăng nhập.
        /// </summary>
        /// <param name="returnUrl">URL chuyển về sau khi đăng nhập thành công.</param>
        /// <returns>View đăng nhập.</returns>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdminLoginViewModel());
        }
        /// <summary>
        /// Xử lý đăng nhập và thiết lập cookie xác thực.
        /// </summary>
        /// <param name="model">Email và mật khẩu.</param>
        /// <param name="returnUrl">URL chuyển về sau khi thành công.</param>
        /// <returns>Chuyển hướng hoặc lại form nếu sai thông tin.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var email = model.Email?.Trim() ?? "";
            var hashedPassword = CryptHelper.HashMD5(model.Password ?? "");
            var employee = await HRDataService.GetEmployeeByEmailAsync(email);
            var passwordMatched = employee != null &&
                                  !string.IsNullOrWhiteSpace(employee.Password) &&
                                  (employee.Password == hashedPassword || employee.Password == model.Password);
            if (employee == null || !employee.IsWorking || !passwordMatched)
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
        /// Đăng xuất và xóa cookie phiên làm việc.
        /// </summary>
        /// <returns>Chuyển về trang đăng nhập.</returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Trang thông báo khi người dùng không có quyền truy cập.
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Hiển thị thông tin cá nhân nhân viên đang đăng nhập.
        /// </summary>
        /// <returns>View hồ sơ.</returns>
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        /// <param name="fullName">Họ và tên cần cập nhật</param>
        /// <param name="email">Địa chỉ email cần cập nhật.</param>
        /// <param name="phone">Số điện thoại cần cập nhật.</param>
        /// <param name="address">Địa chỉ cần cập nhật.</param>
        /// <returns>Chuyển về trang chủ sau khi lưu (demo).</returns>
        [HttpPost]
        public IActionResult Profile(string fullName, string email, string phone, string address)
        {
            // Xử lý cập nhật profile
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Form đổi mật khẩu cho tài khoản đang đăng nhập.
        /// </summary>
        /// <returns>View đổi mật khẩu.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            if (!TryGetCurrentEmployeeId(out var id))
                return Challenge();

            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null || !employee.IsWorking)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            ViewBag.DisplayName = employee.FullName ?? employee.Email;
            ViewBag.Email = employee.Email;
            return View(new AdminChangePasswordViewModel());
        }

        /// <summary>
        /// Cập nhật mật khẩu nhân viên đang đăng nhập (MD5, có kiểm tra mật khẩu cũ).
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel model)
        {
            if (!TryGetCurrentEmployeeId(out var id))
                return Challenge();

            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null || !employee.IsWorking)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            ViewBag.DisplayName = employee.FullName ?? employee.Email;
            ViewBag.Email = employee.Email;

            if (!ModelState.IsValid)
                return View(model);

            var oldHashed = CryptHelper.HashMD5(model.OldPassword ?? "");
            var oldOk = !string.IsNullOrWhiteSpace(employee.Password) &&
                        (employee.Password == oldHashed || employee.Password == model.OldPassword);
            if (!oldOk)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            var ok = await HRDataService.ChangeEmployeePasswordAsync(id, CryptHelper.HashMD5(model.NewPassword ?? ""));
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật mật khẩu. Vui lòng thử lại.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(ChangePassword));
        }

        private bool TryGetCurrentEmployeeId(out int id)
        {
            id = 0;
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out id) && id > 0;
        }
    }
}
