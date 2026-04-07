using SV22T1020536.Admin;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Sales;

namespace SV22T1020536.Admin.AppCodes;

/// <summary>
/// Giỏ hàng lập đơn (Admin POS) — lưu trong session.
/// </summary>
public static class ShoppingCartService
{
    private const string CartKey = "AdminPosShoppingCart";
    /// <summary>
    /// Lấy giỏ hàng từ session
    /// </summary>
    /// <returns></returns>
    public static List<OrderDetailViewInfo> GetShoppingCart()
    {
        var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CartKey);
        if (cart == null)
        {
            cart = new List<OrderDetailViewInfo>();
            ApplicationContext.SetSessionData(CartKey, cart);
        }

        return cart;
    }
    /// <summary>
    /// Lấy thông tin 1 mặt hàng từ giỏ hàng
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>

    public static OrderDetailViewInfo? GetCartItem(int productId)
    {
        return GetShoppingCart().Find(m => m.ProductID == productId);
    }
    /// <summary>
    /// Thêm hàng vào giỏ hàng
    /// </summary>
    /// <param name="item"></param>

    public static void AddCartItem(OrderDetailViewInfo item)
    {
        var cart = GetShoppingCart();
        var exists = cart.Find(m => m.ProductID == item.ProductID);
        if (exists == null)
        {
            cart.Add(item);
        }
        else
        {
            exists.Quantity += item.Quantity;
            exists.SalePrice = item.SalePrice;
            exists.ProductName = item.ProductName;
            exists.Unit = item.Unit;
            exists.Photo = item.Photo;
        }

        ApplicationContext.SetSessionData(CartKey, cart);
    }
    /// <summary>
        /// Cập nhật số lượng và giá của một mặt hàng trong giỏ hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <param name="salePrice"></param>

    public static void UpdateCartItem(int productId, int quantity, decimal salePrice)
    {
        var cart = GetShoppingCart();
        var item = cart.Find(m => m.ProductID == productId);
        if (item == null)
            return;

        item.Quantity = quantity < 1 ? 1 : quantity;
        item.SalePrice = salePrice < 0 ? 0 : salePrice;
        ApplicationContext.SetSessionData(CartKey, cart);
    }
    /// <summary>
    /// Xóa một mặt hàng ra khỏi giỏ hàng
    /// </summary>
    /// <param name="productID"></param>

    public static void RemoveCartItem(int productId)
    {
        var cart = GetShoppingCart();
        var index = cart.FindIndex(m => m.ProductID == productId);
        if (index >= 0)
        {
            cart.RemoveAt(index);
            ApplicationContext.SetSessionData(CartKey, cart);
        }
    }
    /// <summary>
    /// Xóa giỏ hàng
    /// </summary>

    public static void ClearCart()
    {
        ApplicationContext.SetSessionData(CartKey, new List<OrderDetailViewInfo>());
    }

    public static OrderDetailViewInfo FromProduct(Product product, int quantity, decimal salePrice)
    {
        return new OrderDetailViewInfo
        {
            OrderID = 0,
            ProductID = product.ProductID,
            ProductName = product.ProductName,
            Unit = product.Unit,
            Photo = product.Photo ?? "",
            Quantity = quantity < 1 ? 1 : quantity,
            SalePrice = salePrice < 0 ? 0 : salePrice
        };
    }
}
