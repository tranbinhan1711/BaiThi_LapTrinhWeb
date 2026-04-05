using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SV22T1020536.Shop.ViewModels;

public class CheckoutViewModel
{
    [Required]
    [Display(Name = "Tỉnh/Thành giao hàng")]
    public string DeliveryProvince { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Địa chỉ giao hàng")]
    public string DeliveryAddress { get; set; } = string.Empty;

    public List<SelectListItem> ProvinceOptions { get; set; } = new();
}
