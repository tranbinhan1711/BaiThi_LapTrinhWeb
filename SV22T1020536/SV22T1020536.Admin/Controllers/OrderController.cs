using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020536.Admin.AppCodes;
using SV22T1020536.Admin.Models;
using SV22T1020536.BusinessLayers;
using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.Partner;
using SV22T1020536.Models.Sales;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// Quản lý đơn hàng: tra cứu, lập đơn POS, chi tiết và các thao tác trạng thái.
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const int POS_PRODUCT_PAGE_SIZE = 5;

        private int ResolveEmployeeId()
        {
            var claim = User.FindFirstValue("EmployeeID") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out var id) && id > 0)
                return id;
            return 1;
        }

        /// <summary>
        /// Trang nền danh sách đơn hàng (tìm kiếm AJAX ở partial).
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Trả về partial kết quả lọc đơn theo từ khóa, trạng thái và khoảng ngày.
        /// </summary>
        public async Task<IActionResult> Search(int page = 1, string searchValue = "", int status = 0, string dateFrom = "", string dateTo = "")
        {
            var input = new OrderSearchInput
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? "",
                Status = (OrderStatusEnum)status
            };
            if (DateTime.TryParse(dateFrom, out var df))
                input.DateFrom = df.Date;
            if (DateTime.TryParse(dateTo, out var dt))
                input.DateTo = dt.Date;

            var data = await SalesDataService.ListOrdersAsync(input);
            return PartialView(data);
        }

        /// <summary>
        /// Màn hình lập đơn POS: chọn mặt hàng, giỏ tạm và thông tin giao hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(string searchValue = "", int page = 1)
        {
            var model = await BuildCreatePageModelAsync(searchValue, page);
            return View(model);
        }

        /// <summary>
        /// Thêm một dòng hàng vào giỏ lập đơn.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            int productId,
            int quantity,
            decimal salePrice,
            string searchValue = "",
            int page = 1)
        {
            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy mặt hàng.";
                return RedirectToCreate(searchValue, page);
            }

            var line = ShoppingCartService.FromProduct(product, quantity, salePrice);
            ShoppingCartService.AddCartItem(line);
            TempData["Success"] = "Đã thêm mặt hàng vào giỏ.";
            return RedirectToCreate(searchValue, page);
        }

        /// <summary>
        /// Cập nhật số lượng và đơn giá của dòng trong giỏ.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCartItem(
            int productId,
            int quantity,
            decimal salePrice,
            string searchValue = "",
            int page = 1)
        {
            ShoppingCartService.UpdateCartItem(productId, quantity, salePrice);
            return RedirectToCreate(searchValue, page);
        }

        /// <summary>
        /// Xác nhận xóa một dòng khỏi giỏ (GET).
        /// </summary>
        [HttpGet]
        public IActionResult RemoveCartItem(int productId, string searchValue = "", int page = 1)
        {
            var item = ShoppingCartService.GetCartItem(productId);
            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy mặt hàng trong giỏ.";
                return RedirectToCreate(searchValue, page);
            }

            var model = new PosCartRemoveItemConfirmModel
            {
                Line = item,
                SearchValue = searchValue ?? "",
                Page = page
            };
            return View(model);
        }

        /// <summary>
        /// Xóa một dòng khỏi giỏ sau khi xác nhận.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCartItem(int productId, string searchValue = "", int page = 1, IFormCollection? _ = null)
        {
            ShoppingCartService.RemoveCartItem(productId);
            TempData["Success"] = "Đã xóa mặt hàng khỏi giỏ.";
            return RedirectToCreate(searchValue, page);
        }

        /// <summary>
        /// Xác nhận xóa toàn bộ giỏ lập đơn (GET).
        /// </summary>
        [HttpGet]
        public IActionResult ClearPosCart(string searchValue = "", int page = 1)
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToCreate(searchValue, page);
            }

            var model = new PosCartClearConfirmModel
            {
                Cart = cart,
                SearchValue = searchValue ?? "",
                Page = page
            };
            return View(model);
        }

        /// <summary>
        /// Xóa toàn bộ giỏ lập đơn.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearPosCart(string searchValue = "", int page = 1, IFormCollection? _ = null)
        {
            ShoppingCartService.ClearCart();
            TempData["Success"] = "Đã xóa giỏ hàng.";
            return RedirectToCreate(searchValue, page);
        }

        /// <summary>
        /// Ghi đơn hàng xuống CSDL, tạo khách POS và chi tiết dòng từ giỏ.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            string CustomerName,
            string DeliveryProvince,
            string DeliveryAddress,
            string searchValue = "",
            int page = 1)
        {
            var model = await BuildCreatePageModelAsync(searchValue, page);
            model.CustomerName = CustomerName?.Trim() ?? "";
            model.DeliveryProvince = DeliveryProvince?.Trim() ?? "";
            model.DeliveryAddress = DeliveryAddress?.Trim() ?? "";

            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng đang trống.");
            }

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(OrderCreatePageModel.CustomerName), "Vui lòng nhập tên khách hàng.");

            if (string.IsNullOrWhiteSpace(model.DeliveryProvince))
                ModelState.AddModelError(nameof(OrderCreatePageModel.DeliveryProvince), "Vui lòng chọn tỉnh/thành giao hàng.");

            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                ModelState.AddModelError(nameof(OrderCreatePageModel.DeliveryAddress), "Vui lòng nhập địa chỉ giao hàng.");

            var provinces = await DictionaryDataService.ListProvincesAsync();
            if (!string.IsNullOrWhiteSpace(model.DeliveryProvince) &&
                !provinces.Any(p => string.Equals(p.ProvinceName, model.DeliveryProvince, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(OrderCreatePageModel.DeliveryProvince), "Tỉnh/thành không hợp lệ.");
            }

            if (!ModelState.IsValid)
                return View("Create", model);

            // Customers.Email = nvarchar(50). Chuỗi pos.{Guid}@internal.local = 51 ký tự → SqlException truncation
            string email;
            do
            {
                email = $"{Guid.NewGuid():N}@pos";
            } while (!await PartnerDataService.ValidatelCustomerEmailAsync(email));

            var customerId = await PartnerDataService.AddCustomerAsync(new Customer
            {
                CustomerName = model.CustomerName,
                ContactName = model.CustomerName,
                Email = email,
                Password = "",
                Province = model.DeliveryProvince,
                Address = model.DeliveryAddress,
                Phone = "",
                IsLocked = false
            });

            if (customerId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Không thể tạo khách hàng.");
                return View("Create", model);
            }

            var orderId = await SalesDataService.AddOrderAsync(new Order
            {
                CustomerID = customerId,
                DeliveryProvince = model.DeliveryProvince,
                DeliveryAddress = model.DeliveryAddress
            });

            if (orderId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng.");
                return View("Create", model);
            }

            foreach (var line in cart)
            {
                await SalesDataService.AddDetailAsync(new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = line.ProductID,
                    Quantity = line.Quantity,
                    SalePrice = line.SalePrice
                });
            }

            ShoppingCartService.ClearCart();
            TempData["Success"] = $"Đã lập đơn hàng #{orderId}.";
            return RedirectToAction(nameof(Detail), new { id = orderId });
        }

        private IActionResult RedirectToCreate(string searchValue, int page)
        {
            return RedirectToAction(nameof(Create), new { searchValue, page });
        }

        private async Task<OrderCreatePageModel> BuildCreatePageModelAsync(string searchValue, int page)
        {
            page = page < 1 ? 1 : page;
            var input = new ProductSearchInput
            {
                SearchValue = searchValue ?? "",
                Page = page,
                PageSize = POS_PRODUCT_PAGE_SIZE,
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };

            var products = await CatalogDataService.ListProductsAsync(input);
            var provinces = await DictionaryDataService.ListProvincesAsync();
            var options = new List<SelectListItem>
            {
                new("-- Chọn Tỉnh/thành giao hàng --", "")
            };
            foreach (var p in provinces.OrderBy(x => x.ProvinceName))
                options.Add(new SelectListItem(p.ProvinceName, p.ProvinceName));

            return new OrderCreatePageModel
            {
                Products = products,
                SearchValue = searchValue ?? "",
                Page = page,
                Cart = ShoppingCartService.GetShoppingCart(),
                ProvinceOptions = options
            };
        }

        /// <summary>
        /// Chi tiết một đơn hàng và các dòng hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            var details = await SalesDataService.ListDetailsAsync(id);
            var model = new OrderDetailPageModel
            {
                Order = order,
                Details = details
            };
            return View(model);
        }

        /// <summary>
        /// Partial xác nhận duyệt đơn (chấp nhận).
        /// </summary>
        [HttpGet]
        public IActionResult Accept(int id)
        {
            return PartialView(id);
        }

        /// <summary>
        /// Thực hiện duyệt đơn ở trạng thái mới.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptConfirm(int id)
        {
            var ok = await SalesDataService.AcceptOrderAsync(id, ResolveEmployeeId());
            TempData[ok ? "Success" : "Error"] = ok
                ? "Đã duyệt chấp nhận đơn hàng."
                : "Không thể duyệt đơn (đơn không ở trạng thái mới hoặc không tồn tại).";
            return RedirectToAction(nameof(Detail), new { id });
        }

        /// <summary>
        /// Chọn người giao hàng (partial/modal).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            var shippers = await PartnerDataService.ListShippersAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 100,
                SearchValue = ""
            });

            var options = new List<SelectListItem>
            {
                new("— Chọn người giao hàng —", "0")
            };
            options.AddRange(shippers.DataItems.Select(s =>
                new SelectListItem($"{s.ShipperName} ({s.Phone ?? "—"})", s.ShipperID.ToString())));

            var model = new OrderShippingModalModel
            {
                OrderId = id,
                ShipperOptions = options
            };
            return PartialView(model);
        }

        /// <summary>
        /// Gán shipper và chuyển đơn sang trạng thái đang giao.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            if (shipperID <= 0)
            {
                TempData["Error"] = "Vui lòng chọn người giao hàng.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var ok = await SalesDataService.ShipOrderAsync(id, shipperID);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Đã chuyển đơn hàng cho người giao hàng."
                : "Không thể chuyển giao hàng (đơn phải đã được duyệt).";
            return RedirectToAction(nameof(Detail), new { id });
        }

        /// <summary>
        /// Partial xác nhận hoàn tất đơn.
        /// </summary>
        [HttpGet]
        public IActionResult Finish(int id)
        {
            return PartialView(id);
        }

        /// <summary>
        /// Đánh dấu đơn đã hoàn tất sau khi giao.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishConfirm(int id)
        {
            var ok = await SalesDataService.CompleteOrderAsync(id);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Đã hoàn tất đơn hàng."
                : "Không thể hoàn tất (đơn phải đang giao hàng).";
            return RedirectToAction(nameof(Detail), new { id });
        }

        /// <summary>
        /// Partial xác nhận từ chối đơn mới.
        /// </summary>
        [HttpGet]
        public IActionResult Reject(int id)
        {
            return PartialView(id);
        }

        /// <summary>
        /// Từ chối đơn ở trạng thái mới.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectConfirm(int id)
        {
            var ok = await SalesDataService.RejectOrderAsync(id, ResolveEmployeeId());
            TempData[ok ? "Success" : "Error"] = ok
                ? "Đã từ chối đơn hàng."
                : "Không thể từ chối đơn (đơn không ở trạng thái mới).";
            return RedirectToAction(nameof(Detail), new { id });
        }

        /// <summary>
        /// Partial xác nhận hủy đơn.
        /// </summary>
        [HttpGet]
        public IActionResult Cancel(int id)
        {
            return PartialView(id);
        }

        /// <summary>
        /// Hủy đơn theo quy tắc nghiệp vụ.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirm(int id)
        {
            var ok = await SalesDataService.CancelOrderAsync(id);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Đã hủy đơn hàng."
                : "Không thể hủy đơn ở trạng thái hiện tại.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        /// <summary>
        /// Partial xác nhận xóa đơn.
        /// </summary>
        [HttpGet]
        public IActionResult Delete(int id)
        {
            return PartialView(id);
        }

        /// <summary>
        /// Xóa đơn khỏi hệ thống khi được phép.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var ok = await SalesDataService.DeleteOrderAsync(id);
            if (ok)
            {
                TempData["Success"] = "Đã xóa đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Không thể xóa đơn hàng.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
