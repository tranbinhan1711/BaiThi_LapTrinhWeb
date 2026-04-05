using System.Linq;
using System.Threading;
using SV22T1020536.DataLayers.Interfaces;
using SV22T1020536.DataLayers.SqlServer;
using SV22T1020536.Models.DataDictionary;

namespace SV22T1020536.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến từ điển dữ liệu
    /// </summary>
    public static class DictionaryDataService
    {
        private static readonly IDataDictionaryRepository<Province> provinceDB;
        private static readonly SemaphoreSlim ProvinceCacheLock = new(1, 1);
        private static List<Province>? provinceCacheSnapshot;
        private static DateTime provinceCacheExpiryUtc;

        /// <summary>
        /// Ctor
        /// </summary>
        static DictionaryDataService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Lấy danh sách tỉnh thành (có cache trong bộ nhớ để giảm tải SQL; dữ liệu tỉnh ít thay đổi).
        /// </summary>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            if (provinceCacheSnapshot != null && DateTime.UtcNow < provinceCacheExpiryUtc)
                return CopyProvinces(provinceCacheSnapshot);

            await ProvinceCacheLock.WaitAsync();
            try
            {
                if (provinceCacheSnapshot != null && DateTime.UtcNow < provinceCacheExpiryUtc)
                    return CopyProvinces(provinceCacheSnapshot);

                var list = (await provinceDB.ListAsync()).ToList();
                provinceCacheSnapshot = list;
                provinceCacheExpiryUtc = DateTime.UtcNow.AddMinutes(30);
                return CopyProvinces(list);
            }
            finally
            {
                ProvinceCacheLock.Release();
            }
        }

        private static List<Province> CopyProvinces(List<Province> src) =>
            src.Select(p => new Province { ProvinceName = p.ProvinceName }).ToList();
    }
}
