namespace SV22T1020536.Models.Catalog
{
    /// <summary>
    /// Loáº¡i hÃ ng
    /// </summary>
    public class Category
    {
        /// <summary>
        /// MÃ£ loáº¡i hÃ ng
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// TÃªn loáº¡i hÃ ng
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// MÃ´ táº£ loáº¡i hÃ ng
        /// </summary>
        public string? Description { get; set; }
    }
}