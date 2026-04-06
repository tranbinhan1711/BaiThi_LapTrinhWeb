using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// Quản lý mặt hàng: CRUD, thuộc tính và album ảnh.
    /// </summary>
    [Authorize]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 10;

        /// <summary>
        /// Giao diện tìm kiếm và hiển thị danh sách mặt hàng.
        /// </summary>
        /// <returns>Trang danh sách mặt hàng.</returns>
        public async Task<IActionResult> Index()
        {
            var listInput = new PaginationSearchInput { Page = 1, PageSize = 500, SearchValue = "" };
            ViewBag.Categories = (await CatalogDataService.ListCategoriesAsync(listInput)).DataItems;
            ViewBag.Suppliers = (await PartnerDataService.ListSuppliersAsync(listInput)).DataItems;
            return View();
        }

        /// <summary>
        /// Partial kết quả tìm mặt hàng theo bộ lọc và giá.
        /// </summary>
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

        /// <summary>
        /// Nạp danh sách loại hàng và nhà cung cấp cho ViewBag.
        /// </summary>
        private async Task LoadProductLookupsAsync()
        {
            var listInput = new PaginationSearchInput { Page = 1, PageSize = 500, SearchValue = "" };
            ViewBag.Categories = (await CatalogDataService.ListCategoriesAsync(listInput)).DataItems;
            ViewBag.Suppliers = (await PartnerDataService.ListSuppliersAsync(listInput)).DataItems;
        }

        /// <summary>
        /// Chi tiết một mặt hàng kèm tên loại và NCC.
        /// </summary>
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

        /// <summary>
        /// Form thêm mặt hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadProductLookupsAsync();
            return View(new Product { IsSelling = true });
        }

        /// <summary>
        /// Lưu mặt hàng mới sau khi validate.
        /// </summary>
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

        /// <summary>
        /// Form sửa mặt hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            await LoadProductLookupsAsync();
            return View(product);
        }

        /// <summary>
        /// Cập nhật mặt hàng.
        /// </summary>
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

        /// <summary>
        /// Xác nhận xóa mặt hàng (hiển thị tên loại/NCC).
        /// </summary>
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

        /// <summary>
        /// Xóa mặt hàng khi không còn ràng buộc.
        /// </summary>
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
        /// Giao diện danh sách thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <returns>View danh sách thuộc tính.</returns>
        [HttpGet]
        public IActionResult ListAttributes(int id)
        {
            return View();
        }

        /// <summary>
        /// Giao diện thêm mới thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <returns>View tạo thuộc tính.</returns>
        [HttpGet]
        public IActionResult CreateAttribute(int id)
        {
            return View();
        }

        /// <summary>
        /// Xử lý thêm mới thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng cần thêm thuộc tính.</param>
        /// <param name="attributeName">Tên thuộc tính.</param>
        /// <param name="attributeValue">Giá trị thuộc tính.</param>
        /// <param name="displayOrder">Thứ tự hiển thị.</param>
        /// <returns>Chuyển về danh sách thuộc tính.</returns>
        [HttpPost]
        public IActionResult CreateAttribute(int id, string attributeName, string attributeValue, int displayOrder)
        {
            return RedirectToAction("ListAttributes", new { id });
        }

        /// <summary>
        /// Giao diện chỉnh sửa thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="attributeId">Mã thuộc tính.</param>
        /// <returns>View sửa thuộc tính.</returns>
        [HttpGet]
        public IActionResult EditAttribute(int id, long attributeId)
        {
            return View();
        }

        /// <summary>
        /// Xử lý cập nhật thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="attributeId">Mã thuộc tính.</param>
        /// <param name="attributeName">Tên thuộc tính.</param>
        /// <param name="attributeValue">Giá trị thuộc tính.</param>
        /// <param name="displayOrder">Thứ tự hiển thị.</param>
        /// <returns>Chuyển về danh sách thuộc tính.</returns>
        [HttpPost]
        public IActionResult EditAttribute(int id, long attributeId, string attributeName, string attributeValue, int displayOrder)
        {
            return RedirectToAction("ListAttributes", new { id });
        }

        /// <summary>
        /// Giao diện xác nhận xóa thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="attributeId">Mã thuộc tính cần xóa.</param>
        /// <returns>View xác nhận xóa.</returns>
        [HttpGet]
        public IActionResult DeleteAttribute(int id, long attributeId)
        {
            return View();
        }

        /// <summary>
        /// Xử lý xóa thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="attributeId">Mã thuộc tính cần xóa.</param>
        /// <param name="confirm">Xác nhận xóa.</param>
        /// <returns>Chuyển về danh sách thuộc tính.</returns>
        [HttpPost]
        public IActionResult DeleteAttribute(int id, long attributeId, string confirm)
        {
            return RedirectToAction("ListAttributes", new { id });
        }

        // Photos
        /// <summary>
        /// Giao diện danh sách ảnh của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <returns>View danh sách ảnh.</returns>
        [HttpGet]
        public IActionResult ListPhotos(int id)
        {
            return View();
        }

        /// <summary>
        /// Giao diện thêm mới ảnh của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <returns>View thêm ảnh.</returns>
        [HttpGet]
        public IActionResult CreatePhoto(int id)
        {
            return View();
        }

        /// <summary>
        /// Xử lý thêm mới ảnh của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="photo">Đường dẫn ảnh.</param>
        /// <param name="description">Mô tả ảnh.</param>
        /// <param name="displayOrder">Thứ tự hiển thị.</param>
        /// <param name="isHidden">Trạng thái ẩn (true: ẩn, false: hiện).</param>
        /// <returns>Chuyển về danh sách ảnh.</returns>
        [HttpPost]
        public IActionResult CreatePhoto(int id, string photo, string description, int displayOrder, bool isHidden)
        {
            return RedirectToAction("ListPhotos", new { id });
        }

        /// <summary>
        /// Giao diện chỉnh sửa ảnh của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="photoId">Mã ảnh.</param>
        /// <returns>View sửa ảnh.</returns>
        [HttpGet]
        public IActionResult EditPhoto(int id, long photoId)
        {
            return View();
        }

        /// <summary>
        /// Xử lý cập nhật ảnh của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="photoId">Mã ảnh.</param>
        /// <param name="photo">Đường dẫn ảnh.</param>
        /// <param name="description">Mô tả ảnh.</param>
        /// <param name="displayOrder">Thứ tự hiển thị.</param>
        /// <param name="isHidden">Trạng thái ẩn (true: ẩn, false: hiện).</param>
        /// <returns>Chuyển về danh sách ảnh.</returns>
        [HttpPost]
        public IActionResult EditPhoto(int id, long photoId, string photo, string description, int displayOrder, bool isHidden)
        {
            return RedirectToAction("ListPhotos", new { id });
        }

        /// <summary>
        /// Giao diện xác nhận xóa ảnh của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="photoId">Mã ảnh cần xóa.</param>
        /// <returns>View xác nhận xóa.</returns>
        [HttpGet]
        public IActionResult DeletePhoto(int id, long photoId)
        {
            return View();
        }

        /// <summary>
        /// Xử lý xóa ảnh của mặt hàng.
        /// </summary>
        /// <param name="id">Mã mặt hàng.</param>
        /// <param name="photoId">Mã ảnh cần xóa.</param>
        /// <param name="confirm">Xác nhận xóa.</param>
        /// <returns>Chuyển về danh sách ảnh.</returns>
        [HttpPost]
        public IActionResult DeletePhoto(int id, long photoId, string confirm)
        {
            return RedirectToAction("ListPhotos", new { id });
        }

    }
}
