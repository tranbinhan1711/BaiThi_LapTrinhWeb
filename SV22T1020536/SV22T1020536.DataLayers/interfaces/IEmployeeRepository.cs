using SV22T1020536.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020536.DataLayers.Interfaces
{
    /// <summary>
    /// Äá»‹nh nghÄ©a cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u trÃªn Employee
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Kiá»ƒm tra xem email cá»§a nhÃ¢n viÃªn cÃ³ há»£p lá»‡ khÃ´ng
        /// </summary>
        /// <param name="email">Email cáº§n kiá»ƒm tra</param>
        /// <param name="id">
        /// Náº¿u id = 0: Kiá»ƒm tra email cá»§a nhÃ¢n viÃªn má»›i
        /// Náº¿u id <> 0: Kiá»ƒm tra email cá»§a nhÃ¢n viÃªn cÃ³ mÃ£ lÃ  id
        /// </param>
        /// <returns></returns>
        /// <summary>
        /// Lấy nhân viên theo email (dùng cho đăng nhập).
        /// </summary>
        /// <param name="email">Email đăng nhập</param>
        /// <returns></returns>
        Task<Employee?> GetByEmailAsync(string email);

        Task<bool> ValidateEmailAsync(string email, int id = 0);

        /// <summary>
        /// Cập nhật mật khẩu nhân viên
        /// </summary>
        Task<bool> UpdatePasswordAsync(int employeeID, string password);
    }
}
