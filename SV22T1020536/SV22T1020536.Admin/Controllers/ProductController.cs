using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;

namespace SV22T1020536.Admin.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 10;

        /// <summary>
        /// Giao diá»‡n tÃ¬m kiáº¿m vÃ  hiá»ƒn thá»‹ danh sÃ¡ch máº·t hÃ ng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var listInput = new PaginationSearchInput { Page = 1, PageSize = 500, SearchValue = "" };
            ViewBag.Categories = (await CatalogDataService.ListCategoriesAsync(listInput)).DataItems;
            ViewBag.Suppliers = (await PartnerDataService.ListSuppliersAsync(listInput)).DataItems;
            return View();
        }

        public async Task<IActionResult> Search(int page = 1, string searchValue = "", int categoryID = 0, int supplierID = 0, string minPrice = "", string maxPrice = "")
        {
            var input = new ProductSearchInput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? "",
                CategoryID = categoryID,
                SupplierID = supplierID,
                MinPrice = decimal.TryParse(minPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var min) ? min : 0,
                MaxPrice = decimal.TryParse(maxPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var max) ? max : 0
            };
            var data = await CatalogDataService.ListProductsAsync(input);
            return PartialView(data);
        }

        private async Task LoadProductLookupsAsync()
        {
            var listInput = new PaginationSearchInput { Page = 1, PageSize = 500, SearchValue = "" };
            ViewBag.Categories = (await CatalogDataService.ListCategoriesAsync(listInput)).DataItems;
            ViewBag.Suppliers = (await PartnerDataService.ListSuppliersAsync(listInput)).DataItems;
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.CategoryName = product.CategoryID is int cid
                ? (await CatalogDataService.GetCategoryAsync(cid))?.CategoryName
                : null;
            ViewBag.SupplierName = product.SupplierID is int sid
                ? (await PartnerDataService.GetSupplierAsync(sid))?.SupplierName
                : null;

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadProductLookupsAsync();
            return View(new Product { IsSelling = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            await LoadProductLookupsAsync();

            if (string.IsNullOrWhiteSpace(model.ProductName))
                ModelState.AddModelError(nameof(Product.ProductName), "Vui lòng nhập tên mặt hàng.");
            if (string.IsNullOrWhiteSpace(model.Unit))
                ModelState.AddModelError(nameof(Product.Unit), "Vui lòng nhập đơn vị tính.");
            if (model.CategoryID == null || model.CategoryID <= 0)
                ModelState.AddModelError(nameof(Product.CategoryID), "Vui lòng chọn loại hàng.");
            if (model.SupplierID == null || model.SupplierID <= 0)
                ModelState.AddModelError(nameof(Product.SupplierID), "Vui lòng chọn nhà cung cấp.");
            if (model.Price < 0)
                ModelState.AddModelError(nameof(Product.Price), "Giá không hợp lệ.");

            if (!ModelState.IsValid)
                return View(model);

            model.ProductName = model.ProductName.Trim();
            model.ProductDescription = string.IsNullOrWhiteSpace(model.ProductDescription) ? null : model.ProductDescription.Trim();
            model.Unit = model.Unit.Trim();
            model.Photo = string.IsNullOrWhiteSpace(model.Photo) ? null : model.Photo.Trim();

            await CatalogDataService.AddProductAsync(model);
            TempData["SuccessMessage"] = "Đã thêm mặt hàng";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            await LoadProductLookupsAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product input)
        {
            var existing = await CatalogDataService.GetProductAsync(id);
            if (existing == null)
                return NotFound();

            await LoadProductLookupsAsync();
            input.ProductID = id;

            if (string.IsNullOrWhiteSpace(input.ProductName))
                ModelState.AddModelError(nameof(Product.ProductName), "Vui lòng nhập tên mặt hàng.");
            if (string.IsNullOrWhiteSpace(input.Unit))
                ModelState.AddModelError(nameof(Product.Unit), "Vui lòng nhập đơn vị tính.");
            if (input.CategoryID == null || input.CategoryID <= 0)
                ModelState.AddModelError(nameof(Product.CategoryID), "Vui lòng chọn loại hàng.");
            if (input.SupplierID == null || input.SupplierID <= 0)
                ModelState.AddModelError(nameof(Product.SupplierID), "Vui lòng chọn nhà cung cấp.");
            if (input.Price < 0)
                ModelState.AddModelError(nameof(Product.Price), "Giá không hợp lệ.");

            if (!ModelState.IsValid)
                return View(input);

            existing.ProductName = input.ProductName.Trim();
            existing.ProductDescription = string.IsNullOrWhiteSpace(input.ProductDescription) ? null : input.ProductDescription.Trim();
            existing.Unit = input.Unit.Trim();
            existing.Price = input.Price;
            existing.CategoryID = input.CategoryID;
            existing.SupplierID = input.SupplierID;
            existing.IsSelling = input.IsSelling;
            existing.Photo = string.IsNullOrWhiteSpace(input.Photo) ? existing.Photo : input.Photo.Trim();

            await CatalogDataService.UpdateProductAsync(existing);
            TempData["SuccessMessage"] = "Đã cập nhật mặt hàng";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.CategoryName = product.CategoryID is int cid
                ? (await CatalogDataService.GetCategoryAsync(cid))?.CategoryName
                : null;
            ViewBag.SupplierName = product.SupplierID is int sid
                ? (await PartnerDataService.GetSupplierAsync(sid))?.SupplierName
                : null;

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection _)
        {
            var ok = await CatalogDataService.DeleteProductAsync(id);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Không thể xóa mặt hàng (đang được sử dụng hoặc không tồn tại).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa mặt hàng";
            return RedirectToAction(nameof(Index));
        }

        // Attributes
        /// <summary>
        /// Giao diá»‡n danh sÃ¡ch thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListAttributes(int id)
        {
            return View();
        }

        /// <summary>
        /// Giao diá»‡n thÃªm má»›i thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateAttribute(int id)
        {
            return View();
        }

        /// <summary>
        /// Xá»­ lÃ½ thÃªm má»›i thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng cáº§n thÃªm thuá»™c tÃ­nh</param>
        /// <param name="attributeName">TÃªn thuá»™c tÃ­nh cáº§n thÃªm</param>
        /// <param name="attributeValue">GiÃ¡ trá»‹ thuá»™c tÃ­nh cáº§n thÃªm</param>
        /// <param name="displayOrder">Thá»© tá»± hiá»ƒn thá»‹ cáº§n thÃªm</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateAttribute(int id, string attributeName, string attributeValue, int displayOrder)
        {
            return RedirectToAction("ListAttributes", new { id });
        }

        /// <summary>
        /// Giao diá»‡n chá»‰nh sá»­a thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <param name="attributeId">MÃ£ thuá»™c tÃ­nh</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditAttribute(int id, long attributeId)
        {
            return View();
        }

        /// <summary>
        /// Xá»­ lÃ½ cáº­p nháº­t thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng cáº§n cáº­p nháº­t thuá»™c tÃ­nh</param>
        /// <param name="attributeId">MÃ£ thuá»™c tÃ­nh cáº§n cáº­p nháº­t</param>
        /// <param name="attributeName">TÃªn thuá»™c tÃ­nh cáº§n cáº­p nháº­t</param>
        /// <param name="attributeValue">GiÃ¡ trá»‹ thuá»™c tÃ­nh cáº§n cáº­p nháº­t</param>
        /// <param name="displayOrder">Thá»© tá»± hiá»ƒn thá»‹ cáº§n cáº­p nháº­t</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditAttribute(int id, long attributeId, string attributeName, string attributeValue, int displayOrder)
        {
            return RedirectToAction("ListAttributes", new { id });
        }

        /// <summary>
        /// Giao diá»‡n xÃ¡c nháº­n xÃ³a thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <param name="attributeId">MÃ£ thuá»™c tÃ­nh cáº§n xÃ³a</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteAttribute(int id, long attributeId)
        {
            return View();
        }

        /// <summary>
        /// Xá»­ lÃ½ xÃ³a thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <param name="attributeId">MÃ£ thuá»™c tÃ­nh cáº§n xÃ³a</param>
        /// <param name="confirm">XÃ¡c nháº­n xÃ³a</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteAttribute(int id, long attributeId, string confirm)
        {
            return RedirectToAction("ListAttributes", new { id });
        }

        // Photos
        /// <summary>
        /// Giao diá»‡n danh sÃ¡ch áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListPhotos(int id)
        {
            return View();
        }

        /// <summary>
        /// Giao diá»‡n thÃªm má»›i áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreatePhoto(int id)
        {
            return View();
        }

        /// <summary>
        /// Xá»­ lÃ½ thÃªm má»›i áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng cáº§n thÃªm áº£nh</param>
        /// <param name="photo">ÄÆ°á»ng dáº«n áº£nh cáº§n thÃªm</param>
        /// <param name="description">MÃ´ táº£ áº£nh cáº§n thÃªm</param>
        /// <param name="displayOrder">Thá»© tá»± hiá»ƒn thá»‹ cáº§n thÃªm</param>
        /// <param name="isHidden">Tráº¡ng thÃ¡i áº©n cáº§n thÃªm (true: áº©n, false: hiá»‡n)</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreatePhoto(int id, string photo, string description, int displayOrder, bool isHidden)
        {
            return RedirectToAction("ListPhotos", new { id });
        }

        /// <summary>
        /// Giao diá»‡n chá»‰nh sá»­a áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <param name="photoId">MÃ£ áº£nh</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditPhoto(int id, long photoId)
        {
            return View();
        }

        /// <summary>
        /// Xá»­ lÃ½ cáº­p nháº­t áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng cáº§n cáº­p nháº­t áº£nh</param>
        /// <param name="photoId">MÃ£ áº£nh cáº§n cáº­p nháº­t</param>
        /// <param name="photo">ÄÆ°á»ng dáº«n áº£nh cáº§n cáº­p nháº­t</param>
        /// <param name="description">MÃ´ táº£ áº£nh cáº§n cáº­p nháº­t</param>
        /// <param name="displayOrder">Thá»© tá»± hiá»ƒn thá»‹ cáº§n cáº­p nháº­t</param>
        /// <param name="isHidden">Tráº¡ng thÃ¡i áº©n cáº§n cáº­p nháº­t (true: áº©n, false: hiá»‡n)</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditPhoto(int id, long photoId, string photo, string description, int displayOrder, bool isHidden)
        {
            return RedirectToAction("ListPhotos", new { id });
        }

        /// <summary>
        /// Giao diá»‡n xÃ¡c nháº­n xÃ³a áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <param name="photoId">MÃ£ áº£nh cáº§n xÃ³a</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeletePhoto(int id, long photoId)
        {
            return View();
        }

        /// <summary>
        /// Xá»­ lÃ½ xÃ³a áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="id">MÃ£ máº·t hÃ ng</param>
        /// <param name="photoId">MÃ£ áº£nh cáº§n xÃ³a</param>
        /// <param name="confirm">XÃ¡c nháº­n xÃ³a</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeletePhoto(int id, long photoId, string confirm)
        {
            return RedirectToAction("ListPhotos", new { id });
        }

    }
}
