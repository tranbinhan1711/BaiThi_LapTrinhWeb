using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.Models.Common;
using SV22T1020536.Models.Partner;
using System.Data;

namespace SV22T1020536.DataLayers.SqlServer
{
    /// <summary>
    /// CÃ i Ä‘áº·t cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u trÃªn khÃ¡ch hÃ ng Ä‘á»‘i vá»›i SQL Server
    /// </summary>
    public class CustomerRepository : BaseSqlDAL, ICustomerRepository
    {
        // Theo schema LiteCommerceDB (db.mdx): Email/Password nvarchar(50), các trường tên/địa chỉ nvarchar(255)
        private const int EmailMaxLen = 50;
        private const int PasswordMaxLen = 50;
        private const int Nvarchar255MaxLen = 255;

        private static string Truncate(string? value, int maxLen)
        {
            if (string.IsNullOrEmpty(value))
                return value ?? "";
            return value.Length <= maxLen ? value : value[..maxLen];
        }

        /// <summary>
        /// Cột Province FK tới Provinces.ProvinceName — phải gửi SQL NULL khi không chọn, không dùng chuỗi rỗng.
        /// </summary>
        private static string? ProvinceForDb(string? province)
        {
            if (string.IsNullOrWhiteSpace(province))
                return null;
            var t = province.Trim();
            return t.Length <= Nvarchar255MaxLen ? t : t[..Nvarchar255MaxLen];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public CustomerRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// ThÃªm má»›i má»™t khÃ¡ch hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Customer data)
        {
            using (var connection = GetConnection())
            {
                var sql = @"INSERT INTO Customers(CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                            VALUES(@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                            SELECT SCOPE_IDENTITY();";
                var parameters = new
                {
                    CustomerName = Truncate(data.CustomerName, Nvarchar255MaxLen),
                    ContactName = Truncate(data.ContactName, Nvarchar255MaxLen),
                    Province = ProvinceForDb(data.Province),
                    Address = Truncate(data.Address, Nvarchar255MaxLen),
                    Phone = Truncate(data.Phone, Nvarchar255MaxLen),
                    Email = Truncate(data.Email, EmailMaxLen),
                    Password = Truncate(data.Password ?? "", PasswordMaxLen),
                    data.IsLocked
                };
                var id = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return id;
            }
        }

        /// <summary>
        /// XÃ³a má»™t khÃ¡ch hÃ ng theo mÃ£
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = GetConnection())
            {
                var sql = @"DELETE FROM Customers WHERE CustomerID = @CustomerID";
                var parameters = new { CustomerID = id };
                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Láº¥y thÃ´ng tin má»™t khÃ¡ch hÃ ng theo mÃ£
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Customer?> GetAsync(int id)
        {
            using (var connection = GetConnection())
            {
                var sql = @"SELECT * FROM Customers WHERE CustomerID = @CustomerID";
                var parameters = new { CustomerID = id };
                var data = await connection.QueryFirstOrDefaultAsync<Customer>(sql, parameters);
                return data;
            }
        }

        /// <summary>
        /// Kiá»ƒm tra xem khÃ¡ch hÃ ng cÃ³ dá»¯ liá»‡u liÃªn quan hay khÃ´ng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using (var connection = GetConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Orders WHERE CustomerID = @CustomerID)
                                SELECT 1
                            ELSE
                                SELECT 0";
                var parameters = new { CustomerID = id };
                var result = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return result == 1;
            }
        }

        /// <summary>
        /// Danh sÃ¡ch khÃ¡ch hÃ ng cÃ³ phÃ¢n trang vÃ  tÃ¬m kiáº¿m
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            using (var connection = GetConnection())
            {
                var sql = @"SELECT COUNT(*) FROM Customers 
                            WHERE (@SearchValue = N'') 
                               OR (CustomerName LIKE @SearchValue) 
                               OR (ContactName LIKE @SearchValue)
                               OR (Phone LIKE @SearchValue)
                               OR (Email LIKE @SearchValue)
                               OR (Address LIKE @SearchValue);

                            SELECT * FROM Customers 
                            WHERE (@SearchValue = N'') 
                               OR (CustomerName LIKE @SearchValue) 
                               OR (ContactName LIKE @SearchValue)
                               OR (Phone LIKE @SearchValue)
                               OR (Email LIKE @SearchValue)
                               OR (Address LIKE @SearchValue)
                            ORDER BY CustomerName
                            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
                
                var parameters = new
                {
                    SearchValue = $"%{input.SearchValue}%",
                    input.Offset,
                    input.PageSize
                };

                using (var multi = await connection.QueryMultipleAsync(sql, parameters))
                {
                    var rowCount = await multi.ReadFirstAsync<int>();
                    var data = (await multi.ReadAsync<Customer>()).ToList();

                    return new PagedResult<Customer>
                    {
                        Page = input.Page,
                        PageSize = input.PageSize,
                        RowCount = rowCount,
                        DataItems = data
                    };
                }
            }
        }

        public async Task<List<Customer>> ListTopAsync(int take)
        {
            using (var connection = GetConnection())
            {
                var sql = @"SELECT TOP(@Take) * FROM Customers ORDER BY CustomerName";
                return (await connection.QueryAsync<Customer>(sql, new { Take = take })).ToList();
            }
        }

        /// <summary>
        /// Cáº­p nháº­t thÃ´ng tin khÃ¡ch hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using (var connection = GetConnection())
            {
                var sql = @"UPDATE Customers 
                            SET CustomerName = @CustomerName, 
                                ContactName = @ContactName, 
                                Province = @Province, 
                                Address = @Address, 
                                Phone = @Phone, 
                                Email = @Email, 
                                IsLocked = @IsLocked
                            WHERE CustomerID = @CustomerID";
                var parameters = new
                {
                    CustomerName = Truncate(data.CustomerName, Nvarchar255MaxLen),
                    ContactName = Truncate(data.ContactName, Nvarchar255MaxLen),
                    Province = ProvinceForDb(data.Province),
                    Address = Truncate(data.Address, Nvarchar255MaxLen),
                    Phone = Truncate(data.Phone, Nvarchar255MaxLen),
                    Email = Truncate(data.Email, EmailMaxLen),
                    data.IsLocked,
                    data.CustomerID
                };
                var rowsAffected = await connection.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Kiá»ƒm tra email cÃ³ bá»‹ trÃ¹ng hay khÃ´ng
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using (var connection = GetConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Customers WHERE Email = @Email AND CustomerID <> @CustomerID)
                                SELECT 1
                            ELSE
                                SELECT 0";
                var parameters = new { Email = Truncate(email, EmailMaxLen), CustomerID = id };
                var result = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return result == 0; // Tráº£ vá» true náº¿u khÃ´ng bá»‹ trÃ¹ng
            }
        }

        public async Task<bool> UpdatePasswordAsync(int customerID, string password)
        {
            using (var connection = GetConnection())
            {
                var sql = @"UPDATE Customers SET Password = @Password WHERE CustomerID = @CustomerID";
                var rows = await connection.ExecuteAsync(sql, new { CustomerID = customerID, Password = Truncate(password ?? "", PasswordMaxLen) });
                return rows > 0;
            }
        }
    }
}
