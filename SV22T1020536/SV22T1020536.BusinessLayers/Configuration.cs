namespace SV22T1020536.BusinessLayers;

/// <summary>
/// Khá»Ÿi táº¡o vÃ  lÆ°u trá»¯ cÃ¡c giÃ¡ trá»‹ cáº¥u hÃ¬nh cho BusinessLayer
/// </summary>
public static class Configuration
{
    private static string connectionString = "";

    /// <summary>
    /// Khá»Ÿi táº¡o cáº¥u hÃ¬nh
    /// </summary>
    /// <param name="connectionString"></param>
    public static void Initialize(string connectionString)
    {
        Configuration.connectionString = connectionString;
    }

    /// <summary>
    /// Chuá»—i káº¿t ná»‘i Ä‘áº¿n cÆ¡ sá»Ÿ dá»¯ liá»‡u
    /// </summary>
    public static string ConnectionString
    {
        get
        {
            return connectionString;
        }
    }
}
