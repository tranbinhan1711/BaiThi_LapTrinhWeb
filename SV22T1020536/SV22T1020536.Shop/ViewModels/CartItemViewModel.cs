namespace SV22T1020536.Shop.ViewModels;

public class CartItemViewModel
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount => Quantity * Price;
}
