using SV22T1020536.Models.Security;

namespace SV22T1020536.DataLayers.Interfaces
{
    /// <summary>
    /// Äá»‹nh nghÄ©a cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u liÃªn quan Ä‘áº¿n tÃ i khoáº£n
    /// </summary>
    public interface IUserAccountRepository
    {
        /// <summary>
        /// Kiá»ƒm tra xem tÃªn Ä‘Äƒng nháº­p vÃ  máº­t kháº©u cÃ³ há»£p lá»‡ khÃ´ng
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>
        /// Tráº£ vá» thÃ´ng tin cá»§a tÃ i khoáº£n náº¿u thÃ´ng tin Ä‘Äƒng nháº­p há»£p lá»‡,
        /// ngÆ°á»£c láº¡i tráº£ vá» null
        /// </returns>
        Task<UserAccount?> Authorize(string userName, string password);
        /// <summary>
        /// Äá»•i máº­t kháº©u cá»§a tÃ i khoáº£n
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> ChangePassword(string userName, string password);
    }
}
