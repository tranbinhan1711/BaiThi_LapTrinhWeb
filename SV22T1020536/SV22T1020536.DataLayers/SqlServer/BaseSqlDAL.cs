using Dapper;

namespace SV22T1020536.DataLayers.SqlServer
{
    /// <summary>
    /// Lá»›p cÆ¡ sá»Ÿ cho cÃ¡c lá»›p cung cáº¥p dá»¯ liá»‡u trÃªn SQL Server
    /// </summary>
    public abstract class BaseSqlDAL
    {
        /// <summary>
        /// Chuá»—i káº¿t ná»‘i Ä‘áº¿n cÆ¡ sá»Ÿ dá»¯ liá»‡u
        /// </summary>
        protected string _connectionString = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public BaseSqlDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Táº¡o vÃ  má»Ÿ má»™t káº¿t ná»‘i Ä‘áº¿n SQL Server
        /// </summary>
        /// <returns></returns>
        protected Microsoft.Data.SqlClient.SqlConnection GetConnection()
        {
            var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            // Tăng timeout mặc định cho Dapper (COUNT/SELECT có thể nặng).
            SqlMapper.Settings.CommandTimeout = 120;
            connection.Open();
            return connection;
        }
    }
}
