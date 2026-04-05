using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.DataLayers.SqlServer;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;

namespace SV22T1020536.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến danh mục hàng hóa của hệ thống, 
    /// bao gồm: mặt hàng (Product), thuộc tính của mặt hàng (ProductAttribute) và ảnh của mặt hàng (ProductPhoto).
    /// </summary>
    public static class CatalogDataService
    {
        private static readonly ProductRepository productDB;
        private static readonly IGenericRepository<Category> categoryDB;

        /// <summary>Cache danh sách "top N" theo từng giá trị take (Home, sidebar Shop…).</summary>
        private static readonly ConcurrentDictionary<int, (List<Category> Items, DateTime ExpiryUtc)> CategoryTopCache = new();
        private static readonly ConcurrentDictionary<int, (List<Product> Items, DateTime ExpiryUtc)> ProductTopCache = new();
        private const int CategoryTopTtlMinutes = 3;
        private const int ProductTopTtlMinutes = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        static CatalogDataService()
        {
            categoryDB = new CategoryRepository(Configuration.ConnectionString);
            productDB = new ProductRepository(Configuration.ConnectionString);
        }

        private static void InvalidateCategoryTopCache() => CategoryTopCache.Clear();

        private static void InvalidateProductTopCache() => ProductTopCache.Clear();

        #region Category

        /// <summary>
        /// Tìm kiếm và lấy danh sách loại hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang (từ khóa tìm kiếm, trang cần hiển thị, số dòng mỗi trang).
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách loại hàng có phân trang.
        /// </returns>
        public static async Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input)
        {
            return await categoryDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy N loại hàng đầu tiên (không cần RowCount) để hiển thị featured trên Home.
        /// </summary>
        public static async Task<List<Category>> ListCategoriesTopAsync(int take)
        {
            var now = DateTime.UtcNow;
            if (CategoryTopCache.TryGetValue(take, out var ce) && now < ce.ExpiryUtc)
                return ce.Items.ToList();

            var list = await categoryDB.ListTopAsync(take);
            now = DateTime.UtcNow;
            var entry = (list, now.AddMinutes(CategoryTopTtlMinutes));
            CategoryTopCache.AddOrUpdate(take, entry, (_, _) => entry);
            return list.ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một loại hàng dựa vào mã loại hàng.
        /// </summary>
        /// <param name="CategoryID">Mã loại hàng cần tìm.</param>
        /// <returns>
        /// Đối tượng Category nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Category?> GetCategoryAsync(int CategoryID)
        {
            return await categoryDB.GetAsync(CategoryID);
        }

        /// <summary>
        /// Bổ sung một loại hàng mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần bổ sung.</param>
        /// <returns>Mã loại hàng được tạo mới.</returns>
        public static async Task<int> AddCategoryAsync(Category data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            var id = await categoryDB.AddAsync(data);
            if (id > 0)
                InvalidateCategoryTopCache();
            return id;
        }

        /// <summary>
        /// Cập nhật thông tin của một loại hàng.
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateCategoryAsync(Category data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            var ok = await categoryDB.UpdateAsync(data);
            if (ok)
                InvalidateCategoryTopCache();
            return ok;
        }

        /// <summary>
        /// Xóa một loại hàng dựa vào mã loại hàng.
        /// </summary>
        /// <param name="CategoryID">Mã loại hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu loại hàng đang được sử dụng
        /// hoặc việc xóa không thực hiện được.
        /// </returns>
        public static async Task<bool> DeleteCategoryAsync(int CategoryID)
        {
            if (await categoryDB.IsUsedAsync(CategoryID))
                return false;

            var ok = await categoryDB.DeleteAsync(CategoryID);
            if (ok)
                InvalidateCategoryTopCache();
            return ok;
        }

        /// <summary>
        /// Kiểm tra xem một loại hàng có đang được sử dụng trong dữ liệu hay không.
        /// </summary>
        /// <param name="CategoryID">Mã loại hàng cần kiểm tra.</param>
        /// <returns>
        /// True nếu loại hàng đang được sử dụng, ngược lại False.
        /// </returns>
        public static async Task<bool> IsUsedCategoryAsync(int CategoryID)
        {
            return await categoryDB.IsUsedAsync(CategoryID);
        }

        #endregion

        #region Product

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang mặt hàng.
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách mặt hàng có phân trang.
        /// </returns>
        public static async Task<PagedResult<Product>> ListProductsAsync(ProductSearchInput input)
        {
            return await productDB.ListAsync(input);
        }

        /// <summary>
        /// Danh sách theo phân trang nhưng KHÔNG chạy COUNT(*).
        /// Dùng cho Shop Product để load nhanh khi dữ liệu lớn.
        /// </summary>
        public static async Task<PagedResult<Product>> ListProductsPageWithoutCountAsync(ProductSearchInput input)
        {
            // productDB là ProductRepository concrete (không cần sửa interface).
            return await productDB.ListPageWithoutCountAsync(input);
        }

        /// <summary>
        /// Lấy N sản phẩm đầu tiên (không cần RowCount) để hiển thị featured trên Home.
        /// </summary>
        public static async Task<List<Product>> ListProductsTopAsync(int take)
        {
            var now = DateTime.UtcNow;
            if (ProductTopCache.TryGetValue(take, out var pe) && now < pe.ExpiryUtc)
                return pe.Items.ToList();

            var list = await productDB.ListTopAsync(take);
            now = DateTime.UtcNow;
            var entry = (list, now.AddMinutes(ProductTopTtlMinutes));
            ProductTopCache.AddOrUpdate(take, entry, (_, _) => entry);
            return list.ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần tìm.</param>
        /// <returns>
        /// Đối tượng Product nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<Product?> GetProductAsync(int productID)
        {
            return await productDB.GetAsync(productID);
        }

        /// <summary>
        /// Bổ sung một mặt hàng mới vào hệ thống.
        /// </summary>
        /// <param name="data">Thông tin mặt hàng cần bổ sung.</param>
        /// <returns>Mã mặt hàng được tạo mới.</returns>
        public static async Task<int> AddProductAsync(Product data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            var id = await productDB.AddAsync(data);
            if (id > 0)
                InvalidateProductTopCache();
            return id;
        }

        /// <summary>
        /// Cập nhật thông tin của một mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin mặt hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateProductAsync(Product data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            var ok = await productDB.UpdateAsync(data);
            if (ok)
                InvalidateProductTopCache();
            return ok;
        }

        /// <summary>
        /// Xóa một mặt hàng dựa vào mã mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, False nếu mặt hàng đang được sử dụng
        /// hoặc việc xóa không thực hiện được.
        /// </returns>
        public static async Task<bool> DeleteProductAsync(int productID)
        {
            if (await productDB.IsUsedAsync(productID))
                return false;

            var ok = await productDB.DeleteAsync(productID);
            if (ok)
                InvalidateProductTopCache();
            return ok;
        }

        /// <summary>
        /// Kiểm tra xem một mặt hàng có đang được sử dụng trong dữ liệu hay không.
        /// </summary>
        /// <param name="productID">Mã mặt hàng cần kiểm tra.</param>
        /// <returns>
        /// True nếu mặt hàng đang được sử dụng, ngược lại False.
        /// </returns>
        public static async Task<bool> IsUsedProductAsync(int productID)
        {
            return await productDB.IsUsedAsync(productID);
        }

        #endregion

        #region ProductAttribute

        /// <summary>
        /// Lấy danh sách các thuộc tính của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>
        /// Danh sách các thuộc tính của mặt hàng.
        /// </returns>
        public static async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            return await productDB.ListAttributesAsync(productID);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính.</param>
        /// <returns>
        /// Đối tượng ProductAttribute nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            return await productDB.GetAttributeAsync(attributeID);
        }

        /// <summary>
        /// Bổ sung một thuộc tính mới cho mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin thuộc tính cần bổ sung.</param>
        /// <returns>Mã thuộc tính được tạo mới.</returns>
        public static async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            //TODO: Kiểm tra dữ liệu hợp lệ
            return await productDB.AddAttributeAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một thuộc tính mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin thuộc tính cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            return await productDB.UpdateAttributeAsync(data);
        }

        /// <summary>
        /// Xóa một thuộc tính của mặt hàng.
        /// </summary>
        /// <param name="attributeID">Mã thuộc tính cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            return await productDB.DeleteAttributeAsync(attributeID);
        }

        #endregion

        #region ProductPhoto

        /// <summary>
        /// Lấy danh sách ảnh của một mặt hàng.
        /// </summary>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>
        /// Danh sách ảnh của mặt hàng.
        /// </returns>
        public static async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            return await productDB.ListPhotosAsync(productID);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một ảnh của mặt hàng.
        /// </summary>
        /// <param name="photoID">Mã ảnh.</param>
        /// <returns>
        /// Đối tượng ProductPhoto nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            return await productDB.GetPhotoAsync(photoID);
        }

        /// <summary>
        /// Bổ sung một ảnh mới cho mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin ảnh cần bổ sung.</param>
        /// <returns>Mã ảnh được tạo mới.</returns>
        public static async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            return await productDB.AddPhotoAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin của một ảnh mặt hàng.
        /// </summary>
        /// <param name="data">Thông tin ảnh cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            return await productDB.UpdatePhotoAsync(data);
        }

        /// <summary>
        /// Xóa một ảnh của mặt hàng.
        /// </summary>
        /// <param name="photoID">Mã ảnh cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeletePhotoAsync(long photoID)
        {
            return await productDB.DeletePhotoAsync(photoID);
        }

        #endregion
    }
}