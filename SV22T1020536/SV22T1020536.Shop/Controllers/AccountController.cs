using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Partner;
using SV22T1020536.Shop.AppCodes;
using SV22T1020536.Shop.ViewModels;

namespace SV22T1020536.Shop.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!await PartnerDataService.ValidatelCustomerEmailAsync(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
            return View(model);
        }

        var customer = new Customer
        {
            CustomerName = model.ContactName,
            ContactName = model.ContactName,
            Email = model.Email,
            Password = model.Password,
            IsLocked = false
        };

        var customerId = await PartnerDataService.AddCustomerAsync(customer);
        if (customerId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản.");
            return View(model);
        }

        ShopContext.SetCurrentCustomer(HttpContext, new CustomerSessionData
        {
            CustomerID = customerId,
            CustomerName = customer.CustomerName,
            ContactName = customer.ContactName,
            Email = customer.Email
        });

        return RedirectToAction("Index", "Product");
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (ShopContext.GetCurrentCustomer(HttpContext) != null)
            return RedirectToAction("Index", "Product");
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var customers = await PartnerDataService.ListCustomersAsync(new SV22T1020536.Models.Common.PaginationSearchInput
        {
            Page = 1,
            PageSize = 100,
            SearchValue = model.Email
        });

        var customer = customers.DataItems.FirstOrDefault(x => x.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)
                                                               && !x.IsLocked
                                                               && string.Equals(x.Password, model.Password));
        if (customer == null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        ShopContext.SetCurrentCustomer(HttpContext, new CustomerSessionData
        {
            CustomerID = customer.CustomerID,
            CustomerName = customer.CustomerName,
            ContactName = customer.ContactName,
            Email = customer.Email
        });
        return RedirectToAction("Index", "Product");
    }

    public IActionResult Logout()
    {
        ShopContext.ClearCurrentCustomer(HttpContext);
        ShopContext.ClearCart(HttpContext);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction(nameof(Login));

        var customer = await PartnerDataService.GetCustomerAsync(currentCustomer.CustomerID);
        if (customer == null)
        {
            ShopContext.ClearCurrentCustomer(HttpContext);
            return RedirectToAction(nameof(Login));
        }

        var provinces = await DictionaryDataService.ListProvincesAsync();
        var provinceItems = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Chọn tỉnh/thành --" }
        };
        foreach (var p in provinces)
            provinceItems.Add(new SelectListItem { Value = p.ProvinceName, Text = p.ProvinceName });

        ViewBag.Provinces = provinceItems;

        return View(new ProfileViewModel
        {
            CustomerID = customer.CustomerID,
            CustomerName = customer.CustomerName,
            ContactName = customer.ContactName,
            Province = customer.Province,
            Address = customer.Address,
            Phone = customer.Phone,
            Email = customer.Email
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            var provinces = await DictionaryDataService.ListProvincesAsync();
            var provinceItems = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Chọn tỉnh/thành --" }
            };
            foreach (var p in provinces)
                provinceItems.Add(new SelectListItem { Value = p.ProvinceName, Text = p.ProvinceName });
            ViewBag.Provinces = provinceItems;
            return View(model);
        }

        if (!await PartnerDataService.ValidatelCustomerEmailAsync(model.Email, model.CustomerID))
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
            var provinces = await DictionaryDataService.ListProvincesAsync();
            var provinceItems = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Chọn tỉnh/thành --" }
            };
            foreach (var p in provinces)
                provinceItems.Add(new SelectListItem { Value = p.ProvinceName, Text = p.ProvinceName });
            ViewBag.Provinces = provinceItems;
            return View(model);
        }

        var provinceValue = string.IsNullOrWhiteSpace(model.Province)
            ? null
            : model.Province.Trim();

        if (provinceValue != null)
        {
            var provinces = await DictionaryDataService.ListProvincesAsync();
            var exists = provinces.Any(p => string.Equals(p.ProvinceName, provinceValue, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                ModelState.AddModelError(nameof(model.Province), "Tỉnh/Thành không hợp lệ.");
                var provinceItems = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- Chọn tỉnh/thành --" }
                };
                foreach (var p in provinces)
                    provinceItems.Add(new SelectListItem { Value = p.ProvinceName, Text = p.ProvinceName });
                ViewBag.Provinces = provinceItems;
                return View(model);
            }

            model.Province = provinceValue;
        }

        var customer = await PartnerDataService.GetCustomerAsync(model.CustomerID);
        if (customer == null || customer.CustomerID != currentCustomer.CustomerID)
            return RedirectToAction(nameof(Login));

        customer.CustomerName = model.CustomerName;
        customer.ContactName = model.ContactName;
        customer.Province = provinceValue;
        customer.Address = model.Address;
        customer.Phone = model.Phone;
        customer.Email = model.Email;

        var ok = await PartnerDataService.UpdateCustomerAsync(customer);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Cập nhật thất bại.");
            var provinces = await DictionaryDataService.ListProvincesAsync();
            var provinceItems = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Chọn tỉnh/thành --" }
            };
            foreach (var p in provinces)
                provinceItems.Add(new SelectListItem { Value = p.ProvinceName, Text = p.ProvinceName });
            ViewBag.Provinces = provinceItems;
            return View(model);
        }

        // Đảm bảo có danh sách tỉnh/thành để render lại trang Profile sau khi update thành công.
        var successProvinces = await DictionaryDataService.ListProvincesAsync();
        var successProvinceItems = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Chọn tỉnh/thành --" }
        };
        foreach (var p in successProvinces)
            successProvinceItems.Add(new SelectListItem { Value = p.ProvinceName, Text = p.ProvinceName });
        ViewBag.Provinces = successProvinceItems;

        ShopContext.SetCurrentCustomer(HttpContext, new CustomerSessionData
        {
            CustomerID = customer.CustomerID,
            CustomerName = customer.CustomerName,
            ContactName = customer.ContactName,
            Email = customer.Email
        });
        ViewBag.SuccessMessage = "Đã cập nhật thông tin cá nhân.";
        return View(model);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        if (ShopContext.GetCurrentCustomer(HttpContext) == null)
            return RedirectToAction(nameof(Login));
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var currentCustomer = ShopContext.GetCurrentCustomer(HttpContext);
        if (currentCustomer == null)
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
            return View(model);

        var customer = await PartnerDataService.GetCustomerAsync(currentCustomer.CustomerID);
        if (customer == null || customer.Password != model.CurrentPassword)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không chính xác.");
            return View(model);
        }

        var ok = await PartnerDataService.ChangeCustomerPasswordAsync(currentCustomer.CustomerID, model.NewPassword);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Đổi mật khẩu thất bại.");
            return View(model);
        }

        ViewBag.SuccessMessage = "Đổi mật khẩu thành công.";
        ModelState.Clear();
        return View(new ChangePasswordViewModel());
    }
}
