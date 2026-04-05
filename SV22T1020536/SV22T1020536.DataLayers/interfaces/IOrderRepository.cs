using SV22T1020536.Models.Common;
using SV22T1020536.Models.Sales;

namespace SV22T1020536.DataLayers.Interfaces
{
    /// <summary>
    /// Äá»‹nh nghÄ©a cÃ¡c chá»©c nÄƒng xá»­ lÃ½ dá»¯ liá»‡u cho Ä‘Æ¡n hÃ ng
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// TÃ¬m kiáº¿m vÃ  láº¥y danh sÃ¡ch Ä‘Æ¡n hÃ ng dÆ°á»›i dáº¡ng phÃ¢n trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input);
        /// <summary>
        /// Láº¥y thÃ´ng tin 1 Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        Task<OrderViewInfo?> GetAsync(int orderID);
        /// <summary>
        /// Bá»• sung Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns>MÃ£ Ä‘Æ¡n hÃ ng Ä‘Æ°á»£c bá»• sung</returns>
        Task<int> AddAsync(Order data);
        /// <summary>
        /// Cáº­p nháº­t Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(Order data);
        /// <summary>
        /// XÃ³a Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(int orderID);


        /// <summary>
        /// Láº¥y danh sÃ¡ch máº·t hÃ ng trong Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="orderID">MÃ£ Ä‘Æ¡n hÃ ng</param>
        /// <returns></returns>
        Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID);
        /// <summary>
        /// Láº¥y thÃ´ng tin chi tiáº¿t cá»§a má»™t máº·t hÃ ng trong má»™t Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID);
        /// <summary>
        /// Bá»• sung máº·t hÃ ng vÃ o Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> AddDetailAsync(OrderDetail data);
        /// <summary>
        /// Cáº­p nháº­t sá»‘ lÆ°á»£ng vÃ  giÃ¡ bÃ¡n cá»§a má»™t máº·t hÃ ng trong Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateDetailAsync(OrderDetail data);
        /// <summary>
        /// XÃ³a má»™t máº·t hÃ ng khá»i Ä‘Æ¡n hÃ ng
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<bool> DeleteDetailAsync(int orderID, int productID);
    }
}
