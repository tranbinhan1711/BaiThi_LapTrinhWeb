using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SV22T1020536.Shop.Models;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Shop.ViewModels;

namespace SV22T1020536.Shop.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Lấy "featured" cho Home không cần COUNT(*)/phân trang -> load nhanh hơn.
        var categoriesTask = CatalogDataService.ListCategoriesTopAsync(9);
        var productsTask = CatalogDataService.ListProductsTopAsync(6);
        await Task.WhenAll(categoriesTask, productsTask);

        var categories = await categoriesTask;
        var featuredProducts = await productsTask;

        var model = new HomePageViewModel
        {
            Categories = categories.Take(9).ToList(),
            FeaturedProducts = featuredProducts.Take(6).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
