using Microsoft.AspNetCore.Mvc;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Shop.AppCodes;
using SV22T1020536.Shop.ViewModels;

namespace SV22T1020536.Shop.Controllers;

public class CartController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(ShopContext.GetCart(HttpContext));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productID, int quantity = 1)
    {
        var product = await CatalogDataService.GetProductAsync(productID);
        if (product == null || !product.IsSelling)
            return RedirectToAction("Index", "Product");

        var cart = ShopContext.GetCart(HttpContext);
        var item = cart.FirstOrDefault(x => x.ProductID == productID);
        if (item == null)
        {
            cart.Add(new CartItemViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo,
                Price = product.Price,
                Quantity = quantity < 1 ? 1 : quantity
            });
        }
        else
        {
            item.Quantity += quantity < 1 ? 1 : quantity;
        }

        ShopContext.SaveCart(HttpContext, cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productID, int quantity)
    {
        var cart = ShopContext.GetCart(HttpContext);
        var item = cart.FirstOrDefault(x => x.ProductID == productID);
        if (item != null)
        {
            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;
            ShopContext.SaveCart(HttpContext, cart);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productID)
    {
        var cart = ShopContext.GetCart(HttpContext);
        cart.RemoveAll(x => x.ProductID == productID);
        ShopContext.SaveCart(HttpContext, cart);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Trang xác nhận xóa toàn bộ giỏ hàng.</summary>
    [HttpGet]
    public IActionResult ClearConfirm()
    {
        var cart = ShopContext.GetCart(HttpContext);
        if (!cart.Any())
            return RedirectToAction(nameof(Index));

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        ShopContext.ClearCart(HttpContext);
        return RedirectToAction(nameof(Index));
    }
}
