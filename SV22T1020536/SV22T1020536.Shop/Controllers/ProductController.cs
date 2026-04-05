using Microsoft.AspNetCore.Mvc;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;
using SV22T1020536.Shop.ViewModels;
using System.Collections.Concurrent;
using System.Linq;

namespace SV22T1020536.Shop.Controllers;

public class ProductController : Controller
{
    private const int PAGE_SIZE = 12;
    private static readonly ConcurrentDictionary<string, (int LastPage, DateTime ExpiresAt)> _lastPageCache = new();
    private static readonly TimeSpan LastPageCacheTtl = TimeSpan.FromMinutes(5);

    private static string BuildCacheKey(ProductSearchInput input)
    {
        return $"{input.SearchValue}|{input.CategoryID}|{input.MinPrice}|{input.MaxPrice}|{input.PageSize}";
    }

    private async Task<bool> HasNextPageAsync(ProductSearchInput baseInput, int pageToCheck)
    {
        var probeInput = new ProductSearchInput
        {
            SearchValue = baseInput.SearchValue,
            CategoryID = baseInput.CategoryID,
            SupplierID = baseInput.SupplierID,
            MinPrice = baseInput.MinPrice,
            MaxPrice = baseInput.MaxPrice,
            Page = pageToCheck,
            PageSize = baseInput.PageSize
        };

        var raw = await CatalogDataService.ListProductsPageWithoutCountAsync(probeInput);
        return raw.DataItems.Count > baseInput.PageSize;
    }

    private async Task<int> ComputeLastPageAsync(ProductSearchInput input)
    {
        // Nếu lấy trang 1 không còn trang tiếp theo => last = 1
        if (!await HasNextPageAsync(input, 1))
            return 1;

        int low = 1;
        int high = 2;
        int safety = 0;

        // Exponential search để tìm high sao cho high có no-next
        while (await HasNextPageAsync(input, high))
        {
            low = high;
            high *= 2;
            safety++;
            if (safety > 20 || high > 10000) // tránh OFFSET quá lớn
                break;
        }

        // Nếu high vượt giới hạn mà vẫn có next (do safety), dùng high làm xấp xỉ
        if (await HasNextPageAsync(input, high))
            return high;

        // Binary search between low (has next) và high (no next)
        while (low + 1 < high)
        {
            var mid = (low + high) / 2;
            if (await HasNextPageAsync(input, mid))
                low = mid;
            else
                high = mid;
        }

        return high;
    }

    private List<int> BuildDisplayPages(int currentPage, int lastPage)
    {
        if (lastPage <= 0)
            return new List<int> { 1 };

        if (lastPage <= 11)
        {
            return Enumerable.Range(1, lastPage).ToList();
        }

        // Giống ảnh mẫu: nếu current <= 11 => 1..11 ... last
        if (currentPage <= 11)
        {
            var pages = Enumerable.Range(1, 11).ToList();
            pages.Add(0); // 0 => ellipsis
            pages.Add(lastPage);
            return pages;
        }

        // current > 11 => 1 ... current-1 current current+1 ... last
        int start = Math.Max(2, currentPage - 1);
        int end = Math.Min(lastPage - 1, currentPage + 1);

        var result = new List<int> { 1 };
        result.Add(0);
        for (int p = start; p <= end; p++)
            result.Add(p);
        result.Add(0);
        result.Add(lastPage);
        return result;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchValue, int categoryID = 0, decimal minPrice = 0, decimal maxPrice = 0, int page = 1)
    {
        var input = new ProductSearchInput
        {
            SearchValue = searchValue ?? string.Empty,
            CategoryID = categoryID,
            MinPrice = minPrice < 0 ? 0 : minPrice,
            MaxPrice = maxPrice < 0 ? 0 : maxPrice,
            Page = page < 1 ? 1 : page,
            PageSize = PAGE_SIZE
        };

        // Phân trang thật cho trang sản phẩm:
        // - Không chạy COUNT(*)
        // - Lấy thêm 1 dòng để biết có trang tiếp theo không
        var raw = await CatalogDataService.ListProductsPageWithoutCountAsync(input);
        var hasNext = raw.DataItems.Count > input.PageSize;
        var items = raw.DataItems.Take(input.PageSize).ToList();
        raw.DataItems = items;
        raw.RowCount = items.Count;

        // Tính last page để render dạng như ảnh 1 (có cache để giảm số lần probe)
        var cacheKey = BuildCacheKey(input);
        var now = DateTime.UtcNow;
        int lastPage;
        if (_lastPageCache.TryGetValue(cacheKey, out var cacheEntry) && cacheEntry.ExpiresAt > now)
        {
            lastPage = cacheEntry.LastPage;
        }
        else
        {
            lastPage = await ComputeLastPageAsync(input);
            _lastPageCache[cacheKey] = (lastPage, now.Add(LastPageCacheTtl));
        }

        var displayPages = BuildDisplayPages(input.Page, lastPage);

        // Load danh mục filter nhanh: TOP không COUNT(*)
        var categories = await CatalogDataService.ListCategoriesTopAsync(100);

        var model = new ProductSearchViewModel
        {
            SearchValue = input.SearchValue,
            CategoryID = input.CategoryID,
            MinPrice = input.MinPrice,
            MaxPrice = input.MaxPrice,
            Page = input.Page,
            PageSize = input.PageSize,
            HasNextPage = hasNext,
            LastPage = lastPage,
            DisplayPages = displayPages,
            Result = raw,
            Categories = categories
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var product = await CatalogDataService.GetProductAsync(id);
        if (product == null)
            return NotFound();

        // Related products: lấy TOP không COUNT(*) để tránh truy vấn nặng.
        // Lọc theo CategoryID của sản phẩm hiện tại trong bộ nhớ.
        var candidates = await CatalogDataService.ListProductsTopAsync(50);
        var catId = product.CategoryID ?? 0;

        var model = new ProductDetailViewModel
        {
            Product = product,
            Attributes = await CatalogDataService.ListAttributesAsync(id),
            Photos = await CatalogDataService.ListPhotosAsync(id),
            RelatedProducts = candidates
                .Where(p => p.ProductID != id)
                .Where(p => catId <= 0 || (p.CategoryID ?? 0) == catId)
                .Take(4)
                .ToList()
        };
        return View(model);
    }
}
