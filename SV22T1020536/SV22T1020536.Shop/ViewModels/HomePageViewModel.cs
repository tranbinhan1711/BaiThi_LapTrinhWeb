using SV22T1020536.Models.Catalog;

namespace SV22T1020536.Shop.ViewModels;

public class HomePageViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Product> FeaturedProducts { get; set; } = new();
}

