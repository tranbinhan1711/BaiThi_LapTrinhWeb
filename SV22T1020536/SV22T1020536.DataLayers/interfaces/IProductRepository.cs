using SV22T1020536.Models.Catalog;
using SV22T1020536.Models.Common;

namespace SV22T1020536.DataLayers.Interfaces
{
    /// <summary>
    /// Äá»‹nh nghÄ©a cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u cho máº·t hÃ ng
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// TÃ¬m kiáº¿m vÃ  láº¥y danh sÃ¡ch máº·t hÃ ng dÆ°á»›i dáº¡ng phÃ¢n trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResult<Product>> ListAsync(ProductSearchInput input);

        /// <summary>
        /// Lấy N sản phẩm đầu tiên (không tính tổng) - tối ưu cho màn hình featured/top.
        /// </summary>
        Task<List<Product>> ListTopAsync(int take);
        /// <summary>
        /// Láº¥y thÃ´ng tin 1 máº·t hÃ ng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<Product?> GetAsync(int productID);
        /// <summary>
        /// Bá»• sung máº·t hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns>MÃ£ máº·t hÃ ng Ä‘Æ°á»£c bá»• sung</returns>
        Task<int> AddAsync(Product data);
        /// <summary>
        /// Cáº­p nháº­t máº·t hÃ ng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(Product data);
        /// <summary>
        /// XÃ³a máº·t hÃ ng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(int productID);
        /// <summary>
        /// Kiá»ƒm tra máº·t hÃ ng cÃ³ dá»¯ liá»‡u liÃªn quan khÃ´ng
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        Task<bool> IsUsedAsync(int productID);

        /// <summary>
        /// Láº¥y danh sÃ¡ch thuá»™c tÃ­nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="productID">MÃ£ cá»§a máº·t hÃ ng</param>
        /// <returns></returns>
        Task<List<ProductAttribute>> ListAttributesAsync(int productID);
        /// <summary>
        /// Láº¥y thÃ´ng tin cá»§a má»™t thuá»™c tÃ­nh
        /// </summary>
        /// <param name="attributeID">MÃ£ cá»§a thuá»™c tÃ­nh</param>
        /// <returns></returns>
        Task<ProductAttribute?> GetAttributeAsync(long attributeID);
        /// <summary>
        /// Bá»• sung thuá»™c tÃ­nh
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<long> AddAttributeAsync(ProductAttribute data);
        /// <summary>
        /// Cáº­p nháº­t thuá»™c tÃ­nh
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdateAttributeAsync(ProductAttribute data);
        /// <summary>
        /// XÃ³a thuá»™c tÃ­nh
        /// </summary>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        Task<bool> DeleteAttributeAsync(long attributeID);

        /// <summary>
        /// Láº¥y danh sÃ¡ch áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="productID">MÃ£ máº·t hÃ ng</param>
        /// <returns></returns>
        Task<List<ProductPhoto>> ListPhotosAsync(int productID);
        /// <summary>
        /// Láº¥y thÃ´ng tin 1 áº£nh cá»§a máº·t hÃ ng
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        Task<ProductPhoto?> GetPhotoAsync(long photoID);
        /// <summary>
        /// Bá»• sung áº£nh
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<long> AddPhotoAsync(ProductPhoto data);
        /// <summary>
        /// Cáº­p nháº­t áº£nh
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> UpdatePhotoAsync(ProductPhoto data);
        /// <summary>
        /// XÃ³a áº£nh
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        Task<bool> DeletePhotoAsync(long photoID);
    }
}
