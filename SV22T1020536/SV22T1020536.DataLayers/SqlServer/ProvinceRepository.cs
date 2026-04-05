using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.Models.DataDictionary;
using System.Data;

namespace SV22T1020536.DataLayers.SqlServer
{
    /// <summary>
    /// CÃ i Ä‘áº·t cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u tá»‰nh thÃ nh phá»‘ trÃªn SQL Server
    /// </summary>
    public class ProvinceRepository : BaseSqlDAL, IDataDictionaryRepository<Province>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public ProvinceRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Láº¥y danh sÃ¡ch toÃ n bá»™ cÃ¡c tá»‰nh thÃ nh phá»‘
        /// </summary>
        /// <returns></returns>
        public async Task<List<Province>> ListAsync()
        {
            using (var connection = GetConnection())
            {
                var sql = "SELECT * FROM Provinces ORDER BY ProvinceName";
                var data = (await connection.QueryAsync<Province>(sql)).ToList();
                return data;
            }
        }
    }
}
