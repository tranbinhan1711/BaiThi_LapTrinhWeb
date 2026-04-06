using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SV22T1020536.Admin.AppCodes;
using SV22T1020536.Admin.ViewModels;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.HR;
using System.Security.Claims;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// Xác thực tài khoản nhân viên: đăng nhập, đăng ký, hồ sơ và mật khẩu.
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
        /// Form đăng ký tài khoản nhân viên (mẫu).
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View(new AdminRegisterViewModel());
        }

        /// <summary>
        /// Tạo nhân viên mới với chức vụ được phép (Staff/Manager/Admin).
        /// </summary>
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
                Password = CryptHelper.HashMD5(model.Password ?? ""),
                IsWorking = true,
                RoleNames = roleNames,
            };

            await HRDataService.AddEmployeeAsync(employee);
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
        /// Form đổi mật khẩu.
        /// </summary>
        /// <returns>View đổi mật khẩu.</returns>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đổi mật khẩu (mẫu demo).
        /// </summary>
        /// <param name="oldPassword">Mật khẩu hiện tại.</param>
        /// <param name="newPassword">Mật khẩu mới.</param>
        /// <param name="confirmPassword">Xác nhận mật khẩu mới (phải trùng với mật khẩu mới).</param>
        /// <returns>Chuyển về trang chủ nếu hợp lệ.</returns>
        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // Kiểm tra và đổi mật khẩu (giả lập)
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không trùng khớp.");
                return View();
            }

            // Xử lý đổi mật khẩu thành công...
            return RedirectToAction("Index", "Home");
        }
    }
}
