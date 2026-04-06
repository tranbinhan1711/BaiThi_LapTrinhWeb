using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.Partner;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// CRUD nhà cung cấp.
    /// </summary>
    [Authorize]
    public class SupplierController : Controller
    {
        private const int PAGE_SIZE = 10;

        /// <summary>
        /// Trang danh sách nhà cung cấp (kết quả tải qua partial tìm kiếm).
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Partial phân trang và tìm nhà cung cấp.
        /// </summary>
        public async Task<IActionResult> Search(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? ""
            };
            var data = await PartnerDataService.ListSuppliersAsync(input);
            return PartialView(data);
        }

        /// <summary>
        /// Form thêm nhà cung cấp.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(new Supplier());
        }

        /// <summary>
        /// Lưu nhà cung cấp mới sau khi validate.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier model)
        {
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();

            if (string.IsNullOrWhiteSpace(model.SupplierName))
                ModelState.AddModelError(nameof(Supplier.SupplierName), "Vui lòng nhập tên nhà cung cấp.");
            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError(nameof(Supplier.ContactName), "Vui lòng nhập tên giao dịch.");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError(nameof(Supplier.Phone), "Vui lòng nhập điện thoại.");

            if (!ModelState.IsValid)
                return View(model);

            model.SupplierName = model.SupplierName.Trim();
            model.ContactName = model.ContactName.Trim();
            model.Phone = model.Phone?.Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
            model.Province = string.IsNullOrWhiteSpace(model.Province) ? null : model.Province.Trim();

            await PartnerDataService.AddSupplierAsync(model);
            TempData["SuccessMessage"] = "Đã thêm nhà cung cấp";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Form sửa nhà cung cấp.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await PartnerDataService.GetSupplierAsync(id);
            if (supplier == null)
                return NotFound();

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(supplier);
        }

        /// <summary>
        /// Cập nhật nhà cung cấp.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier input)
        {
            var existing = await PartnerDataService.GetSupplierAsync(id);
            if (existing == null)
                return NotFound();

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            input.SupplierID = id;

            if (string.IsNullOrWhiteSpace(input.SupplierName))
                ModelState.AddModelError(nameof(Supplier.SupplierName), "Vui lòng nhập tên nhà cung cấp.");
            if (string.IsNullOrWhiteSpace(input.ContactName))
                ModelState.AddModelError(nameof(Supplier.ContactName), "Vui lòng nhập tên giao dịch.");
            if (string.IsNullOrWhiteSpace(input.Phone))
                ModelState.AddModelError(nameof(Supplier.Phone), "Vui lòng nhập điện thoại.");

            if (!ModelState.IsValid)
                return View(input);

            existing.SupplierName = input.SupplierName.Trim();
            existing.ContactName = input.ContactName.Trim();
            existing.Phone = input.Phone?.Trim();
            existing.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
            existing.Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim();
            existing.Province = string.IsNullOrWhiteSpace(input.Province) ? null : input.Province.Trim();

            await PartnerDataService.UpdateSupplierAsync(existing);
            TempData["SuccessMessage"] = "Đã cập nhật nhà cung cấp";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Xác nhận xóa nhà cung cấp.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await PartnerDataService.GetSupplierAsync(id);
            if (supplier == null)
                return NotFound();
            return View(supplier);
        }

        /// <summary>
        /// Xóa nhà cung cấp nếu không bị tham chiếu.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection _)
        {
            var ok = await PartnerDataService.DeleteSupplierAsync(id);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp (đang được sử dụng hoặc không tồn tại).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa nhà cung cấp";
            return RedirectToAction(nameof(Index));
        }
    }
}
