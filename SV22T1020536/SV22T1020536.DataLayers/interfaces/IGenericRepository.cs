using SV22T1020536.Models.Common;

namespace SV22T1020536.DataLayers.Interfaces
{
    /// <summary>
    /// Äá»‹nh nghÄ©a cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u Ä‘Æ¡n giáº£n trÃªn má»™t
    /// kiá»ƒu dá»¯ liá»‡u T nÃ o Ä‘Ã³ (T lÃ  má»™t Entity/DomainModel nÃ o Ä‘Ã³)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Truy váº¥n, tÃ¬m kiáº¿m dá»¯ liá»‡u vÃ  tráº£ vá» káº¿t quáº£ dÆ°á»›i dáº¡ng Ä‘Æ°á»£c phÃ¢n trang
        /// </summary>
        /// <param name="input">Äáº§u vÃ o tÃ¬m kiáº¿m, phÃ¢n trang</param>
        /// <returns></returns>
        Task<PagedResult<T>> ListAsync(PaginationSearchInput input);

        /// <summary>
        /// Lấy danh sách N dòng đầu tiên (không cần tổng số dòng) - tối ưu cho UI cần "featured/top".
        /// </summary>
        Task<List<T>> ListTopAsync(int take);
        /// <summary>
        /// Láº¥y dá»¯ liá»‡u cá»§a má»™t báº£n ghi cÃ³ mÃ£ lÃ  id (tráº£ vá» null náº¿u khÃ´ng cÃ³ dá»¯ liá»‡u)
        /// </summary>
        /// <param name="id">MÃ£ cá»§a dá»¯ liá»‡u cáº§n láº¥y</param>
        /// <returns></returns>
        Task<T?> GetAsync(int id);
        /// <summary>
        /// Bá»• sung má»™t báº£n ghi vÃ o báº£ng trong CSDL
        /// </summary>
        /// <param name="data">Dá»¯ liá»‡u cáº§n bá»• sung</param>
        /// <returns>MÃ£ cá»§a dÃ²ng dá»¯ liá»‡u Ä‘Æ°á»£c bá»• sung (thÆ°á»ng lÃ  IDENTITY)</returns>
        Task<int> AddAsync(T data);
        /// <summary>
        /// Cáº­p nháº­t má»™t báº£n ghi trong báº£ng cá»§a CSDL
        /// </summary>
        /// <param name="data">Dá»¯ liá»‡u cáº§n cáº­p nháº­t</param>
        /// <returns></returns>
        Task<bool> UpdateAsync(T data);
        /// <summary>
        /// XÃ³a báº£n ghi cÃ³ mÃ£ lÃ  id
        /// </summary>
        /// <param name="id">MÃ£ cá»§a báº£n ghi cáº§n xÃ³a</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(int id);
        /// <summary>
        /// Kiá»ƒm tra xem má»™t báº£n ghi cÃ³ mÃ£ lÃ  id cÃ³ dá»¯ liá»‡u liÃªn quan hay khÃ´ng?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> IsUsedAsync(int id);
    }
}
