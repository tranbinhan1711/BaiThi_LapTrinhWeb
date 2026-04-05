using SV22T1020536.Models.Catalog;

namespace SV22T1020536.Shop.ViewModels;

public class ProductDetailViewModel
{
    public Product Product { get; set; } = new();
    public List<ProductAttribute> Attributes { get; set; } = new();
    public List<ProductPhoto> Photos { get; set; } = new();

    public List<Product> RelatedProducts { get; set; } = new();
}
