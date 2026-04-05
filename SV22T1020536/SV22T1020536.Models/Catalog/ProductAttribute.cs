namespace SV22T1020536.Models.Catalog
{
    /// <summary>
    /// Thuá»™c tÃ­nh cá»§a máº·t hÃ ng
    /// </summary>
    public class ProductAttribute
    {
        /// <summary>
        /// MÃ£ thuá»™c tÃ­nh
        /// </summary>
        public long AttributeID { get; set; }
        /// <summary>
        /// MÃ£ máº·t hÃ ng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// TÃªn thuá»™c tÃ­nh (vÃ­ dá»¥: "MÃ u sáº¯c", "KÃ­ch thÆ°á»›c", "Cháº¥t liá»‡u", ...)
        /// </summary>
        public string AttributeName { get; set; } = string.Empty;
        /// <summary>
        /// GiÃ¡ trá»‹ thuá»™c tÃ­nh
        /// </summary>
        public string AttributeValue { get; set; } = string.Empty;
        /// <summary>
        /// Thá»© tá»± hiá»ƒn thá»‹ thuá»™c tÃ­nh (giÃ¡ trá»‹ nhá» sáº½ hiá»ƒn thá»‹ trÆ°á»›c)
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}