using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.HR;

namespace SV22T1020536.Admin.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private const int PAGE_SIZE = 10;

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? ""
            };
            var data = await HRDataService.ListEmployeesAsync(input);
            return PartialView(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Employee { IsWorking = true, RoleNames = "Staff" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee model)
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError(nameof(Employee.FullName), "Vui lòng nhập họ tên.");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(Employee.Email), "Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(Employee.Password), "Vui lòng nhập mật khẩu.");
            if (string.IsNullOrWhiteSpace(model.RoleNames))
                ModelState.AddModelError(nameof(Employee.RoleNames), "Vui lòng chọn chức vụ.");

            model.Email = model.Email?.Trim() ?? "";
            if (!ModelState.IsValid)
                return View(model);

            if (!await HRDataService.ValidateEmployeeEmailAsync(model.Email, 0))
            {
                ModelState.AddModelError(nameof(Employee.Email), "Email đã được sử dụng.");
                return View(model);
            }

            model.FullName = model.FullName.Trim();
            model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            model.RoleNames = model.RoleNames.Trim();
            model.Photo = string.IsNullOrWhiteSpace(model.Photo) ? null : model.Photo.Trim();

            await HRDataService.AddEmployeeAsync(model);
            TempData["SuccessMessage"] = "Đã thêm nhân viên";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();
            employee.Password = null;
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee input)
        {
            var existing = await HRDataService.GetEmployeeAsync(id);
            if (existing == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(input.FullName))
                ModelState.AddModelError(nameof(Employee.FullName), "Vui lòng nhập họ tên.");
            if (string.IsNullOrWhiteSpace(input.Email))
                ModelState.AddModelError(nameof(Employee.Email), "Vui lòng nhập email.");
            if (string.IsNullOrWhiteSpace(input.RoleNames))
                ModelState.AddModelError(nameof(Employee.RoleNames), "Vui lòng chọn chức vụ.");

            input.Email = input.Email?.Trim() ?? "";
            if (!ModelState.IsValid)
            {
                input.Password = null;
                return View(input);
            }

            if (!await HRDataService.ValidateEmployeeEmailAsync(input.Email, id))
            {
                ModelState.AddModelError(nameof(Employee.Email), "Email đã được sử dụng.");
                input.Password = null;
                return View(input);
            }

            existing.FullName = input.FullName.Trim();
            existing.BirthDate = input.BirthDate;
            existing.Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim();
            existing.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
            existing.Email = input.Email;
            existing.IsWorking = input.IsWorking;
            existing.RoleNames = input.RoleNames.Trim();
            existing.Photo = string.IsNullOrWhiteSpace(input.Photo) ? existing.Photo : input.Photo.Trim();

            await HRDataService.UpdateEmployeeAsync(existing);
            TempData["SuccessMessage"] = "Đã cập nhật nhân viên";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();
            employee.Password = null;
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection _)
        {
            var ok = await HRDataService.DeleteEmployeeAsync(id);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể xóa nhân viên (đang có đơn hàng liên quan hoặc không tồn tại).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa nhân viên";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();
            employee.Password = null;
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError(string.Empty, "Vui lòng nhập mật khẩu mới.");
            if (newPassword != confirmPassword)
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");

            if (!ModelState.IsValid)
            {
                employee.Password = null;
                return View(employee);
            }

            var ok = await HRDataService.ChangeEmployeePasswordAsync(id, newPassword.Trim());
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể cập nhật mật khẩu.";
                employee.Password = null;
                return View(employee);
            }

            TempData["SuccessMessage"] = "Đã đổi mật khẩu nhân viên";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChangeRole(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();
            employee.Password = null;
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string roleNames)
        {
            var existing = await HRDataService.GetEmployeeAsync(id);
            if (existing == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(roleNames))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một quyền / chức vụ.");
                existing.Password = null;
                return View(existing);
            }

            existing.RoleNames = roleNames.Trim();
            await HRDataService.UpdateEmployeeAsync(existing);
            TempData["SuccessMessage"] = "Đã cập nhật phân quyền nhân viên";
            return RedirectToAction(nameof(Index));
        }
    }
}
