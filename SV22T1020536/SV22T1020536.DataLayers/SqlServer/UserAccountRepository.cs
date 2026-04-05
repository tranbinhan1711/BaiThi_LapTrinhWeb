using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.Models.Security;
using System.Data;

namespace SV22T1020536.DataLayers.SqlServer
{
    /// <summary>
    /// CÃ i Ä‘áº·t cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u liÃªn quan Ä‘áº¿n tÃ i khoáº£n ngÆ°á»i dÃ¹ng Ä‘á»‘i vá»›i SQL Server
    /// </summary>
    public class UserAccountRepository : BaseSqlDAL, IUserAccountRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public UserAccountRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// XÃ¡c thá»±c tÃ i khoáº£n ngÆ°á»i dÃ¹ng
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            using (var connection = GetConnection())
            {
                // LÆ°u Ã½: Trong thá»±c táº¿ nÃªn hash máº­t kháº©u. á»ž Ä‘Ã¢y lÃ m theo database hiá»‡n táº¡i.
                var sql = @"SELECT EmployeeID AS UserID, Email AS UserName, FullName, Photo, RoleNames 
                            FROM Employees 
                            WHERE Email = @Email AND Password = @Password AND IsWorking = 1";
                var parameters = new { Email = userName, Password = password };
                var user = await connection.QueryFirstOrDefaultAsync<UserAccount>(sql, parameters);
                return user;
            }
        }

        /// <summary>
        /// Äá»•i máº­t kháº©u
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> ChangePassword(string userName, string password)
        {
            using (var connection = GetConnection())
            {
                var sql = @"UPDATE Employees SET Password = @Password WHERE Email = @Email";
                var parameters = new { Email = userName, Password = password };
                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }
    }
}
