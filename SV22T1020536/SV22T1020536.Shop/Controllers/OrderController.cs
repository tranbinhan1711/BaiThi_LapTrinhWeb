using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Sales;
using SV22T1020536.Shop.AppCodes;
using SV22T1020536.Shop.ViewModels;

namespace SV22T1020536.Shop.Controllers;

public class OrderController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction("Login", "Account");

        var cart = ShopContext.GetCart(HttpContext);
        if (cart.Count == 0)
            return RedirectToAction("Index", "Cart");

        // Prefill thông tin giao hàng từ hồ sơ khách hàng để không phải nhập lại.
        // Lưu ý: dữ liệu trong session chỉ có CustomerID/Name/Email, nên cần query lại từ DB.
        var customer = await PartnerDataService.GetCustomerAsync(currentCustomer.CustomerID);
        var model = new CheckoutViewModel
        {
            DeliveryProvince = customer?.Province ?? string.Empty,
            DeliveryAddress = customer?.Address ?? string.Empty
        };
        await PopulateProvinceOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction("Login", "Account");

        var cart = ShopContext.GetCart(HttpContext);
        if (cart.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Giỏ hàng đang trống.");
            await PopulateProvinceOptionsAsync(model);
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            await PopulateProvinceOptionsAsync(model);
            return View(model);
        }

        var provinces = await DictionaryDataService.ListProvincesAsync();
        if (!string.IsNullOrWhiteSpace(model.DeliveryProvince) &&
            !provinces.Any(p => string.Equals(p.ProvinceName, model.DeliveryProvince, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(CheckoutViewModel.DeliveryProvince), "Tỉnh/thành không hợp lệ.");
            await PopulateProvinceOptionsAsync(model);
            return View(model);
        }

        var orderId = await SalesDataService.AddOrderAsync(new Order
        {
            CustomerID = currentCustomer.CustomerID,
            DeliveryProvince = model.DeliveryProvince,
            DeliveryAddress = model.DeliveryAddress
        });

        if (orderId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng.");
            await PopulateProvinceOptionsAsync(model);
            return View(model);
        }

        foreach (var item in cart)
        {
            await SalesDataService.AddDetailAsync(new OrderDetail
            {
                OrderID = orderId,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                SalePrice = item.Price
            });
        }

        ShopContext.ClearCart(HttpContext);
        return RedirectToAction(nameof(Detail), new { id = orderId });
    }

    private static async Task PopulateProvinceOptionsAsync(CheckoutViewModel model)
    {
        var provinces = await DictionaryDataService.ListProvincesAsync();
        var options = new List<SelectListItem>
        {
            new("-- Chọn Tỉnh/thành giao hàng --", "")
        };
        foreach (var p in provinces
                     .Where(x => !string.IsNullOrWhiteSpace(x.ProvinceName))
                     .OrderBy(x => x.ProvinceName))
            options.Add(new SelectListItem(p.ProvinceName, p.ProvinceName));

        model.ProvinceOptions = options;
    }

    [HttpGet]
    public async Task<IActionResult> History(int page = 1, int status = 0)
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction("Login", "Account");

        const int pageSize = 10;
        var allOrders = await SalesDataService.ListOrdersAsync(new OrderSearchInput
        {
            Page = 1,
            PageSize = 100
        });

        var filtered = allOrders.DataItems
            .Where(x => x.CustomerID == currentCustomer.CustomerID)
            .OrderByDescending(x => x.OrderTime)
            .ToList();

        if (status != 0)
        {
            var s = (OrderStatusEnum)status;
            filtered = filtered.Where(x => x.Status == s).ToList();
        }

        page = page < 1 ? 1 : page;
        var result = new SV22T1020536.Models.Common.PagedResult<OrderViewInfo>
        {
            Page = page,
            PageSize = pageSize,
            RowCount = filtered.Count,
            DataItems = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };

        ViewBag.SelectedStatus = status;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction("Login", "Account");

        var order = await SalesDataService.GetOrderAsync(id);
        if (order == null || order.CustomerID != currentCustomer.CustomerID)
            return NotFound();

        ViewBag.Details = await SalesDataService.ListDetailsAsync(id);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction("Login", "Account");

        var order = await SalesDataService.GetOrderAsync(id);
        if (order == null || order.CustomerID != currentCustomer.CustomerID)
            return NotFound();

        await SalesDataService.CancelOrderAsync(id);
        return RedirectToAction(nameof(Detail), new { id });
    }
}
