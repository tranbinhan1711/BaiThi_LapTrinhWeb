using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;

namespace SV22T1020536.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
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
            var data = await CatalogDataService.ListCategoriesAsync(input);
            return PartialView(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (string.IsNullOrWhiteSpace(model.CategoryName))
                ModelState.AddModelError(nameof(Category.CategoryName), "Vui lòng nhập tên loại hàng.");

            if (!ModelState.IsValid)
                return View(model);

            model.CategoryName = model.CategoryName.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            await CatalogDataService.AddCategoryAsync(model);
            TempData["SuccessMessage"] = "Đã thêm loại hàng";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await CatalogDataService.GetCategoryAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category input)
        {
            var existing = await CatalogDataService.GetCategoryAsync(id);
            if (existing == null)
                return NotFound();

            input.CategoryID = id;

            if (string.IsNullOrWhiteSpace(input.CategoryName))
                ModelState.AddModelError(nameof(Category.CategoryName), "Vui lòng nhập tên loại hàng.");

            if (!ModelState.IsValid)
                return View(input);

            existing.CategoryName = input.CategoryName.Trim();
            existing.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();

            await CatalogDataService.UpdateCategoryAsync(existing);
            TempData["SuccessMessage"] = "Đã cập nhật loại hàng";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await CatalogDataService.GetCategoryAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection _)
        {
            var ok = await CatalogDataService.DeleteCategoryAsync(id);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể xóa loại hàng (đang được sử dụng hoặc không tồn tại).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa loại hàng";
            return RedirectToAction(nameof(Index));
        }
    }
}
