namespace SV22T1020536.DataLayers.Interfaces
{
    /// <summary>
    /// Äá»‹nh nghÄ©a cÃ¡c phÃ©p xá»­ lÃ½ dá»¯ liá»‡u sá»­ dá»¥ng cho tá»« Ä‘iá»ƒn dá»¯ liá»‡u
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataDictionaryRepository<T> where T : class
    {
        /// <summary>
        /// Láº¥y danh sÃ¡ch dá»¯ liá»‡u
        /// </summary>
        /// <returns></returns>
        Task<List<T>> ListAsync();
    }
}
