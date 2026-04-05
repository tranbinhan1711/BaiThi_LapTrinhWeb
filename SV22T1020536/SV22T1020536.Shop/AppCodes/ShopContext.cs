using SV22T1020536.Shop.ViewModels;

namespace SV22T1020536.Shop.AppCodes;

public static class ShopContext
{
    public const string CurrentCustomerKey = "SHOP.CURRENT_CUSTOMER";
    public const string CartKey = "SHOP.CART";

    public static CustomerSessionData? GetCurrentCustomer(HttpContext httpContext)
    {
        return httpContext.Session.GetObject<CustomerSessionData>(CurrentCustomerKey);
    }

    public static void SetCurrentCustomer(HttpContext httpContext, CustomerSessionData data)
    {
        httpContext.Session.SetObject(CurrentCustomerKey, data);
    }

    public static void ClearCurrentCustomer(HttpContext httpContext)
    {
        httpContext.Session.Remove(CurrentCustomerKey);
    }

    public static List<CartItemViewModel> GetCart(HttpContext httpContext)
    {
        return httpContext.Session.GetObject<List<CartItemViewModel>>(CartKey) ?? new List<CartItemViewModel>();
    }

    public static void SaveCart(HttpContext httpContext, List<CartItemViewModel> cartItems)
    {
        httpContext.Session.SetObject(CartKey, cartItems);
    }

    public static void ClearCart(HttpContext httpContext)
    {
        httpContext.Session.Remove(CartKey);
    }
}
