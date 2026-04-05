using SV22T1020536.Models.Partner;

namespace SV22T1020536.DataLayers.Interfaces
{
    /// <summary>
    /// Äá»‹nh nghÄ©a cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u trÃªn Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiá»ƒm tra xem má»™t Ä‘á»‹a chá»‰ email cÃ³ há»£p lá»‡ hay khÃ´ng?
        /// </summary>
        /// <param name="email">Email cáº§n kiá»ƒm tra</param>
        /// <param name="id">
        /// Náº¿u id = 0: Kiá»ƒm tra email cá»§a khÃ¡ch hÃ ng má»›i.
        /// Náº¿u id <> 0: Kiá»ƒm tra email Ä‘á»‘i vá»›i khÃ¡ch hÃ ng Ä‘Ã£ tá»“n táº¡i
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);

        /// <summary>
        /// Cập nhật mật khẩu khách hàng
        /// </summary>
        Task<bool> UpdatePasswordAsync(int customerID, string password);
    }
}
