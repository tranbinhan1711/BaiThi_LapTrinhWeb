using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.Partner;

namespace SV22T1020536.Admin.Controllers
{
    [Authorize]
    public class ShipperController : Controller
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
            var data = await PartnerDataService.ListShippersAsync(input);
            return PartialView(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Shipper());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shipper model)
        {
            if (string.IsNullOrWhiteSpace(model.ShipperName))
                ModelState.AddModelError(nameof(Shipper.ShipperName), "Vui lòng nhập tên người giao hàng.");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError(nameof(Shipper.Phone), "Vui lòng nhập điện thoại.");

            if (!ModelState.IsValid)
                return View(model);

            model.ShipperName = model.ShipperName.Trim();
            model.Phone = model.Phone?.Trim();

            await PartnerDataService.AddShipperAsync(model);
            TempData["SuccessMessage"] = "Đã thêm người giao hàng";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var shipper = await PartnerDataService.GetShipperAsync(id);
            if (shipper == null)
                return NotFound();
            return View(shipper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shipper input)
        {
            var existing = await PartnerDataService.GetShipperAsync(id);
            if (existing == null)
                return NotFound();

            input.ShipperID = id;

            if (string.IsNullOrWhiteSpace(input.ShipperName))
                ModelState.AddModelError(nameof(Shipper.ShipperName), "Vui lòng nhập tên người giao hàng.");
            if (string.IsNullOrWhiteSpace(input.Phone))
                ModelState.AddModelError(nameof(Shipper.Phone), "Vui lòng nhập điện thoại.");

            if (!ModelState.IsValid)
                return View(input);

            existing.ShipperName = input.ShipperName.Trim();
            existing.Phone = input.Phone?.Trim();

            await PartnerDataService.UpdateShipperAsync(existing);
            TempData["SuccessMessage"] = "Đã cập nhật người giao hàng";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var shipper = await PartnerDataService.GetShipperAsync(id);
            if (shipper == null)
                return NotFound();
            return View(shipper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection _)
        {
            var ok = await PartnerDataService.DeleteShipperAsync(id);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể xóa người giao hàng (đang được sử dụng hoặc không tồn tại).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa người giao hàng";
            return RedirectToAction(nameof(Index));
        }
    }
}
