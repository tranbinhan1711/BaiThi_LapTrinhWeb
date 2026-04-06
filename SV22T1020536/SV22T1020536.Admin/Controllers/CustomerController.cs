using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020536.Admin.AppCodes;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.Partner;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// Quản lý khách hàng, gồm đổi mật khẩu.
    /// </summary>
    [Authorize]
    public class CustomerController : Controller
    {
        private const int PAGE_SIZE = 10;

        /// <summary>Trang danh sách khách hàng.</summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>Partial tìm kiếm khách hàng.</summary>
        public async Task<IActionResult> Search(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue
            };
            var data = await PartnerDataService.ListCustomersAsync(input);
            return PartialView(data);
        }

        /// <summary>Form thêm khách hàng.</summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(new Customer { IsLocked = false });
        }

        /// <summary>Lưu khách hàng mới (kiểm tra email trùng).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer model)
        {
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(Customer.CustomerName), "Vui lòng nhập tên khách hàng.");
            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError(nameof(Customer.ContactName), "Vui lòng nhập tên giao dịch.");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError(nameof(Customer.Phone), "Vui lòng nhập điện thoại.");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(Customer.Email), "Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(Customer.Password), "Vui lòng nhập mật khẩu.");

            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();
            if (!await PartnerDataService.ValidatelCustomerEmailAsync(email, 0))
            {
                ModelState.AddModelError(nameof(Customer.Email), "Email đã được sử dụng bởi khách hàng khác.");
                return View(model);
            }

            model.CustomerName = model.CustomerName.Trim();
            model.ContactName = model.ContactName.Trim();
            model.Phone = model.Phone?.Trim();
            model.Email = email;
            model.Password = CryptHelper.HashMD5(model.Password ?? "");
            model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            model.Province = string.IsNullOrWhiteSpace(model.Province) ? null : model.Province.Trim();
            await PartnerDataService.AddCustomerAsync(model);
            TempData["SuccessMessage"] = "Đã thêm khách hàng thành công.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Form sửa khách hàng.</summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
                return NotFound();

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(customer);
        }

        /// <summary>Cập nhật thông tin khách (email duy nhất).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer input)
        {
            var existing = await PartnerDataService.GetCustomerAsync(id);
            if (existing == null)
                return NotFound();

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();

            if (string.IsNullOrWhiteSpace(input.CustomerName))
                ModelState.AddModelError(nameof(Customer.CustomerName), "Vui lòng nhập tên khách hàng.");
            if (string.IsNullOrWhiteSpace(input.ContactName))
                ModelState.AddModelError(nameof(Customer.ContactName), "Vui lòng nhập tên giao dịch.");
            if (string.IsNullOrWhiteSpace(input.Phone))
                ModelState.AddModelError(nameof(Customer.Phone), "Vui lòng nhập điện thoại.");
            if (string.IsNullOrWhiteSpace(input.Email))
                ModelState.AddModelError(nameof(Customer.Email), "Vui lòng nhập email.");

            input.CustomerID = id;
            input.Email = input.Email?.Trim() ?? "";

            if (!ModelState.IsValid)
            {
                input.Password = existing.Password;
                return View(input);
            }

            if (!await PartnerDataService.ValidatelCustomerEmailAsync(input.Email, id))
            {
                ModelState.AddModelError(nameof(Customer.Email), "Email đã được sử dụng bởi khách hàng khác.");
                input.Password = existing.Password;
                return View(input);
            }

            existing.CustomerName = input.CustomerName.Trim();
            existing.ContactName = input.ContactName.Trim();
            existing.Phone = input.Phone?.Trim();
            existing.Email = input.Email;
            existing.Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim();
            existing.Province = string.IsNullOrWhiteSpace(input.Province) ? null : input.Province.Trim();
            existing.IsLocked = input.IsLocked;

            await PartnerDataService.UpdateCustomerAsync(existing);
            TempData["SuccessMessage"] = "Đã cập nhật khách hàng thành công.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Xác nhận xóa khách hàng.</summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
                return NotFound();
            return View(customer);
        }

        /// <summary>Xóa khách khi không còn ràng buộc dữ liệu.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection _)
        {
            var ok = await PartnerDataService.DeleteCustomerAsync(id);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng (đang có dữ liệu liên quan hoặc không tồn tại).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa khách hàng.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Form đổi mật khẩu đăng nhập shop của khách.</summary>
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
                return NotFound();
            return View(customer);
        }

        /// <summary>Áp dụng mật khẩu mới cho tài khoản khách.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError(string.Empty, "Vui lòng nhập mật khẩu mới.");
            if (newPassword != confirmPassword)
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");

            if (!ModelState.IsValid)
                return View(customer);

            var ok = await PartnerDataService.ChangeCustomerPasswordAsync(id, CryptHelper.HashMD5(newPassword.Trim()));
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể cập nhật mật khẩu.";
                return View(customer);
            }

            TempData["SuccessMessage"] = "Đã đổi mật khẩu khách hàng.";
            return RedirectToAction(nameof(Index));
        }
    }
}
