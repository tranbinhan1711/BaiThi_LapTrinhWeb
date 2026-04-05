using Microsoft.AspNetCore.Mvc;
using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.Models.Common;

namespace SV22T1020536.Admin.Controllers
{
    /// <summary>
    /// Controller Ä‘á»ƒ test chá»©c nÄƒng láº¥y dá»¯ liá»‡u vá»›i phÃ¢n trang vÃ  tÃ¬m kiáº¿m
    /// </summary>
    public class TestController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        /// <summary>
        /// Constructor nháº­n repository thÃ´ng qua Dependency Injection
        /// </summary>
        /// <param name="customerRepository"></param>
        public TestController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// Action Index nháº­n PaginationSearchInput vÃ  gá»i Repository Ä‘á»ƒ láº¥y dá»¯ liá»‡u
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(PaginationSearchInput input)
        {
            // Thiáº¿t láº­p giÃ¡ trá»‹ máº·c Ä‘á»‹nh náº¿u chÆ°a cÃ³
            if (input.PageSize <= 0) input.PageSize = 10;
            if (input.Page <= 0) input.Page = 1;

            // Gá»i repository Ä‘á»ƒ láº¥y dá»¯ liá»‡u phÃ¢n trang
            var result = await _customerRepository.ListAsync(input);

            // Tráº£ vá» view kÃ¨m theo dá»¯ liá»‡u
            return View(result);
        }
    }
}
