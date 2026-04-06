using Microsoft.AspNetCore.Mvc;
using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.Models.Common;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// Controller thử nghiệm lấy dữ liệu khách hàng kèm phân trang và tìm kiếm.
    /// </summary>
    public class TestController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        /// <summary>
        /// Khởi tạo controller với repository khách hàng (dependency injection).
        /// </summary>
        /// <param name="customerRepository">Repository truy vấn khách hàng.</param>
        public TestController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// Hiển thị danh sách khách hàng theo tham số phân trang và tìm kiếm.
        /// </summary>
        /// <param name="input">Trang, kích thước trang và từ khóa tìm kiếm.</param>
        /// <returns>View kết quả phân trang.</returns>
        public async Task<IActionResult> Index(PaginationSearchInput input)
        {
            // Gán kích thước trang mặc định nếu chưa có.
            if (input.PageSize <= 0) input.PageSize = 10;
            if (input.Page <= 0) input.Page = 1;

            // Gọi repository để lấy dữ liệu phân trang.
            var result = await _customerRepository.ListAsync(input);

            // Trả về view kèm dữ liệu.
            return View(result);
        }
    }
}
