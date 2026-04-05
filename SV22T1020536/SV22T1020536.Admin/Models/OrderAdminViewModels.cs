using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.Sales;

namespace SV22T1020536.Admin.Models;

public class OrderCreatePageModel
{
    public PagedResult<Product> Products { get; set; } = new();
    public string SearchValue { get; set; } = "";
    public int Page { get; set; } = 1;
    public List<OrderDetailViewInfo> Cart { get; set; } = new();
    public decimal CartTotal => Cart.Sum(x => x.TotalPrice);

    [Display(Name = "Khách hàng")]
    public string CustomerName { get; set; } = "";

    [Display(Name = "Tỉnh/thành")]
    public string DeliveryProvince { get; set; } = "";

    [Display(Name = "Địa chỉ")]
    public string DeliveryAddress { get; set; } = "";

    public List<SelectListItem> ProvinceOptions { get; set; } = new();
}

public class OrderDetailPageModel
{
    public OrderViewInfo Order { get; set; } = null!;
    public List<OrderDetailViewInfo> Details { get; set; } = new();
}

public class OrderShippingModalModel
{
    public int OrderId { get; set; }
    public List<SelectListItem> ShipperOptions { get; set; } = new();
}

/// <summary>
/// Trang xác nhận xóa một dòng khỏi giỏ lập đơn (POS).
/// </summary>
public class PosCartRemoveItemConfirmModel
{
    public OrderDetailViewInfo Line { get; set; } = null!;
    public string SearchValue { get; set; } = "";
    public int Page { get; set; } = 1;
}

/// <summary>
/// Trang xác nhận xóa toàn bộ giỏ lập đơn (POS).
/// </summary>
public class PosCartClearConfirmModel
{
    public List<OrderDetailViewInfo> Cart { get; set; } = new();
    public string SearchValue { get; set; } = "";
    public int Page { get; set; } = 1;
    public decimal CartTotal => Cart.Sum(x => x.TotalPrice);
}
