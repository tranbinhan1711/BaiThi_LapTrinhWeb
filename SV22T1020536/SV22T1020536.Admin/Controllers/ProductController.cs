using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SV22T1020536.Admin.AppCodes;
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
        private readonly IWebHostEnvironment _env;

        public ProductController(IWebHostEnvironment env)
        {
            _env = env;
        }

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

        private void TryDeleteProductImageFile(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;
            var name = Path.GetFileName(fileName.Trim());
            if (name.Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return;
            var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var productsDir = Path.GetFullPath(Path.Combine(root, "images", "products"));
            var full = Path.GetFullPath(Path.Combine(productsDir, name));
            if (!full.StartsWith(productsDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(full, productsDir, StringComparison.OrdinalIgnoreCase))
                return;
            if (System.IO.File.Exists(full))
            {
                try
                {
                    System.IO.File.Delete(full);
                }
                catch
                {
                    // Bỏ qua nếu file đang bị khóa
                }
            }
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
        public async Task<IActionResult> Create(Product model, IFormFile? photoFile)
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

            var savedPhoto = await WebImageUploadHelper.SaveProductImageAsync(_env, photoFile, ModelState, "photoFile");
            if (!ModelState.IsValid)
                return View(model);

            model.ProductName = model.ProductName.Trim();
            model.ProductDescription = string.IsNullOrWhiteSpace(model.ProductDescription) ? null : model.ProductDescription.Trim();
            model.Unit = model.Unit.Trim();
            model.Photo = savedPhoto;

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
        public async Task<IActionResult> Edit(int id, Product input, IFormFile? photoFile)
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

            var savedPhoto = await WebImageUploadHelper.SaveProductImageAsync(_env, photoFile, ModelState, "photoFile");
            if (!ModelState.IsValid)
                return View(input);

            existing.ProductName = input.ProductName.Trim();
            existing.ProductDescription = string.IsNullOrWhiteSpace(input.ProductDescription) ? null : input.ProductDescription.Trim();
            existing.Unit = input.Unit.Trim();
            existing.Price = input.Price;
            existing.CategoryID = input.CategoryID;
            existing.SupplierID = input.SupplierID;
            existing.IsSelling = input.IsSelling;
            if (savedPhoto != null)
                existing.Photo = savedPhoto;
            else
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

            ViewData["Title"] = "Xác nhận xóa mặt hàng";
            return View(product);
        }

        /// <summary>
        /// Thực hiện xóa mặt hàng sau khi đã xác nhận trên trang Delete.
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

        // Attributes — trang danh sách + CreateAttribute riêng (từ thẻ trên Edit).
        /// <summary>Danh sách thuộc tính của mặt hàng.</summary>
        [HttpGet]
        public async Task<IActionResult> ListAttributes(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();
            ViewBag.Product = product;
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
            ViewData["Title"] = "Thuộc tính mặt hàng";
            return View();
        }

        /// <summary>Form thêm thuộc tính.</summary>
        [HttpGet]
        public async Task<IActionResult> CreateAttribute(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();
            ViewData["Title"] = "Bổ sung thuộc tính cho mặt hàng";
            return View(product);
        }

        /// <summary>Lưu thuộc tính mới.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAttribute(int id, string attributeName, string attributeValue, int displayOrder)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(attributeName) || string.IsNullOrWhiteSpace(attributeValue))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên thuộc tính và giá trị.");
                ViewData["Title"] = "Bổ sung thuộc tính cho mặt hàng";
                return View(product);
            }

            await CatalogDataService.AddAttributeAsync(new ProductAttribute
            {
                ProductID = id,
                AttributeName = attributeName.Trim(),
                AttributeValue = attributeValue.Trim(),
                DisplayOrder = displayOrder
            });
            TempData["SuccessMessage"] = "Đã thêm thuộc tính.";
            return RedirectToAction(nameof(ListAttributes), new { id });
        }

        /// <summary>Form sửa thuộc tính.</summary>
        [HttpGet]
        public async Task<IActionResult> EditAttribute(int id, long attributeId)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            var attr = await CatalogDataService.GetAttributeAsync(attributeId);
            if (product == null || attr == null || attr.ProductID != id)
                return NotFound();
            ViewBag.Product = product;
            ViewData["Title"] = "Sửa thuộc tính mặt hàng";
            return View(attr);
        }

        /// <summary>Cập nhật thuộc tính.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAttribute(int id, long attributeId, string attributeName, string attributeValue, int displayOrder)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            var attr = await CatalogDataService.GetAttributeAsync(attributeId);
            if (product == null || attr == null || attr.ProductID != id)
                return NotFound();

            if (string.IsNullOrWhiteSpace(attributeName) || string.IsNullOrWhiteSpace(attributeValue))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên thuộc tính và giá trị.");
                ViewBag.Product = product;
                ViewData["Title"] = "Sửa thuộc tính mặt hàng";
                return View(attr);
            }

            attr.AttributeName = attributeName.Trim();
            attr.AttributeValue = attributeValue.Trim();
            attr.DisplayOrder = displayOrder;
            await CatalogDataService.UpdateAttributeAsync(attr);
            TempData["SuccessMessage"] = "Đã cập nhật thuộc tính.";
            return RedirectToAction(nameof(ListAttributes), new { id });
        }

        /// <summary>Xác nhận xóa thuộc tính.</summary>
        [HttpGet]
        public async Task<IActionResult> DeleteAttribute(int id, long attributeId)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            var attr = await CatalogDataService.GetAttributeAsync(attributeId);
            if (product == null || attr == null || attr.ProductID != id)
                return NotFound();
            ViewBag.Product = product;
            ViewBag.Attribute = attr;
            ViewData["Title"] = "Xóa thuộc tính mặt hàng";
            return View();
        }

        /// <summary>Xóa thuộc tính.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttribute(int id, long attributeId, IFormCollection _)
        {
            var attr = await CatalogDataService.GetAttributeAsync(attributeId);
            if (attr == null || attr.ProductID != id)
                return NotFound();

            await CatalogDataService.DeleteAttributeAsync(attributeId);
            TempData["SuccessMessage"] = "Đã xóa thuộc tính.";
            return RedirectToAction(nameof(ListAttributes), new { id });
        }

        // Photos — thư viện ảnh: ListPhotos + CreatePhoto (nút Thêm ảnh mở trang upload).
        /// <summary>Danh sách ảnh album.</summary>
        [HttpGet]
        public async Task<IActionResult> ListPhotos(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();
            ViewBag.Product = product;
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewData["Title"] = "Thư viện ảnh";
            return View();
        }

        /// <summary>Trang thêm ảnh (upload file).</summary>
        [HttpGet]
        public async Task<IActionResult> CreatePhoto(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();
            ViewData["Title"] = "Thêm ảnh vào thư viện";
            return View(product);
        }

        /// <summary>Lưu ảnh upload vào thư viện.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePhoto(int id, IFormFile? galleryFile, string? photoTitle, int displayOrder, bool setAsMain = false, bool isHidden = false)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            if (galleryFile == null || galleryFile.Length == 0)
            {
                ModelState.AddModelError("galleryFile", "Vui lòng chọn file ảnh.");
                ViewData["Title"] = "Thêm ảnh vào thư viện";
                return View(product);
            }

            var uploadState = new ModelStateDictionary();
            var fileName = await WebImageUploadHelper.SaveProductGalleryImageAsync(_env, galleryFile, uploadState, "galleryFile");
            if (fileName == null)
            {
                foreach (var err in uploadState.Values.SelectMany(v => v.Errors))
                    ModelState.AddModelError("galleryFile", err.ErrorMessage);
                ViewData["Title"] = "Thêm ảnh vào thư viện";
                return View(product);
            }

            await CatalogDataService.AddPhotoAsync(new ProductPhoto
            {
                ProductID = id,
                Photo = fileName,
                Description = string.IsNullOrWhiteSpace(photoTitle) ? "" : photoTitle.Trim(),
                DisplayOrder = displayOrder,
                IsHidden = isHidden
            });

            if (setAsMain)
            {
                product.Photo = fileName;
                await CatalogDataService.UpdateProductAsync(product);
            }

            TempData["SuccessMessage"] = "Đã thêm ảnh vào thư viện.";
            return RedirectToAction(nameof(ListPhotos), new { id });
        }

        /// <summary>Form sửa metadata / đổi file ảnh trong thư viện.</summary>
        [HttpGet]
        public async Task<IActionResult> EditPhoto(int id, long photoId)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (product == null || photo == null || photo.ProductID != id)
                return NotFound();
            ViewBag.Product = product;
            ViewData["Title"] = "Sửa ảnh trong thư viện";
            return View(photo);
        }

        /// <summary>Cập nhật ảnh thư viện.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoto(int id, long photoId, IFormFile? galleryFile, string description, int displayOrder, bool isHidden = false)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (product == null || photo == null || photo.ProductID != id)
                return NotFound();

            var oldFileName = photo.Photo;

            if (galleryFile != null && galleryFile.Length > 0)
            {
                var uploadState = new ModelStateDictionary();
                var newName = await WebImageUploadHelper.SaveProductGalleryImageAsync(_env, galleryFile, uploadState, "galleryFile");
                if (newName == null)
                {
                    foreach (var err in uploadState.Values.SelectMany(v => v.Errors))
                        ModelState.AddModelError("galleryFile", err.ErrorMessage);
                    ViewBag.Product = product;
                    ViewData["Title"] = "Sửa ảnh trong thư viện";
                    return View(photo);
                }

                TryDeleteProductImageFile(oldFileName);
                photo.Photo = newName;

                if (string.Equals(product.Photo, oldFileName, StringComparison.OrdinalIgnoreCase))
                {
                    product.Photo = newName;
                    await CatalogDataService.UpdateProductAsync(product);
                }
            }

            photo.Description = string.IsNullOrWhiteSpace(description) ? "" : description.Trim();
            photo.DisplayOrder = displayOrder;
            photo.IsHidden = isHidden;
            await CatalogDataService.UpdatePhotoAsync(photo);

            TempData["SuccessMessage"] = "Đã cập nhật ảnh.";
            return RedirectToAction(nameof(ListPhotos), new { id });
        }

        /// <summary>Xác nhận xóa ảnh thư viện.</summary>
        [HttpGet]
        public async Task<IActionResult> DeletePhoto(int id, long photoId)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (product == null || photo == null || photo.ProductID != id)
                return NotFound();
            ViewBag.Product = product;
            ViewBag.Photo = photo;
            ViewData["Title"] = "Xóa ảnh thư viện";
            return View();
        }

        /// <summary>Xóa ảnh thư viện.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(int id, long photoId, IFormCollection _)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (product == null || photo == null || photo.ProductID != id)
                return NotFound();

            var deletedFileName = photo.Photo;
            await CatalogDataService.DeletePhotoAsync(photoId);
            TryDeleteProductImageFile(deletedFileName);

            if (string.Equals(product.Photo, deletedFileName, StringComparison.OrdinalIgnoreCase))
            {
                product.Photo = null;
                await CatalogDataService.UpdateProductAsync(product);
            }

            TempData["SuccessMessage"] = "Đã xóa ảnh.";
            return RedirectToAction(nameof(ListPhotos), new { id });
        }

    }
}
