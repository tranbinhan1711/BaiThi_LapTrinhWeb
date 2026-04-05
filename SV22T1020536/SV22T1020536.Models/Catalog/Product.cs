namespace SV22T1020536.Models.Catalog
{
    /// <summary>
    /// Máº·t hÃ ng
    /// </summary>
    public class Product
    {
        /// <summary>
        /// MÃ£ máº·t hÃ ng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// TÃªn máº·t hÃ ng
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        /// <summary>
        /// MÃ´ táº£ máº·t hÃ ng
        /// </summary>
        public string? ProductDescription { get; set; }
        /// <summary>
        /// MÃ£ nhÃ  cung cáº¥p
        /// </summary>
        public int? SupplierID { get; set; }
        /// <summary>
        /// MÃ£ loáº¡i hÃ ng
        /// </summary>
        public int? CategoryID { get; set; }
        /// <summary>
        /// ÄÆ¡n vá»‹ tÃ­nh
        /// </summary>
        public string Unit { get; set; } = string.Empty;
        /// <summary>
        /// GiÃ¡
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// TÃªn file áº£nh Ä‘áº¡i diá»‡n cá»§a máº·t hÃ ng (náº¿u cÃ³)
        /// </summary>
        public string? Photo { get; set; }
        /// <summary>
        /// Máº·t hÃ ng hiá»‡n cÃ³ Ä‘ang Ä‘Æ°á»£c bÃ¡n hay khÃ´ng?
        /// </summary>
        public bool IsSelling { get; set; }
    }
}
