using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;

namespace SV22T1020536.Shop.ViewModels;

public class ProductSearchViewModel
{
    public string SearchValue { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public bool HasNextPage { get; set; }
    public int LastPage { get; set; } = 1;

    // Dùng cho UI phân trang: số trang hoặc 0 để biểu thị dấu "..."
    public List<int> DisplayPages { get; set; } = new();

    public List<Category> Categories { get; set; } = new();
    public PagedResult<Product> Result { get; set; } = new();
}
