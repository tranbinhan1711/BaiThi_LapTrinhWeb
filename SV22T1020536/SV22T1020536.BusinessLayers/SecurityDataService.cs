using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.DataLayers.SqlServer;
using SV22T1020536.Models.Security;

namespace SV22T1020536.BusinessLayers
{
    /// <summary>
    /// Cung cáº¥p cÃ¡c chá»©c nÄƒng nghiá»‡p vá»¥ liÃªn quan Ä‘áº¿n báº£o máº­t (ÄÄƒng nháº­p, Äá»•i máº­t kháº©u...)
    /// </summary>
    public static class SecurityDataService
    {
        private static readonly IUserAccountRepository userAccountDB;

        /// <summary>
        /// Constructor tÄ©nh
        /// </summary>
        static SecurityDataService()
        {
            string connectionString = Configuration.ConnectionString;
            userAccountDB = new UserAccountRepository(connectionString);
        }

        /// <summary>
        /// Kiá»ƒm tra Ä‘Äƒng nháº­p
        /// </summary>
        /// <param name="userName">TÃªn Ä‘Äƒng nháº­p</param>
        /// <param name="password">Máº­t kháº©u</param>
        /// <returns>ThÃ´ng tin tÃ i khoáº£n náº¿u há»£p lá»‡; ngÆ°á»£c láº¡i tráº£ vá» null</returns>
        public static async Task<UserAccount?> Authorize(string userName, string password)
        {
            return await userAccountDB.Authorize(userName, password);
        }

        /// <summary>
        /// Thá»±c hiá»‡n Ä‘á»•i máº­t kháº©u
        /// </summary>
        /// <param name="userName">TÃªn tÃ i khoáº£n</param>
        /// <param name="password">Máº­t kháº©u má»›i</param>
        /// <returns>True náº¿u thÃ nh cÃ´ng</returns>
        public static async Task<bool> ChangePassword(string userName, string password)
        {
            return await userAccountDB.ChangePassword(userName, password);
        }
    }
}
