namespace SV22T1020536.Models.Common
{
    /// <summary>
    /// Tìm file ảnh thật trong wwwroot/images/products khớp với DB (đuôi khác, mã trong tên SP, v.v.).
    /// </summary>
    public sealed class ProductPhotoPathResolver
    {
        private readonly string _dir;
        private static readonly string[] Extensions = [".webp", ".jpg", ".jpeg", ".png", ".gif"];

        public ProductPhotoPathResolver(string webRootPath)
        {
            _dir = Path.Combine(string.IsNullOrEmpty(webRootPath) ? "" : webRootPath, "images", "products");
        }

        /// <summary>
        /// Trả về đường dẫn ảo ~/images/products/... để dùng với Url.Content, hoặc null nếu không có file.
        /// Giá trị ảnh mặc định trong DB (nophoto.png, demo.png) được bỏ qua để thử mã trong tên hàng (vd. 04528-...).
        /// </summary>
        public string? ResolveVirtualPath(string? photoFromDb, string? productName)
        {
            var hint = photoFromDb;
            if (IsPlaceholderFileName(StripToFileName(hint)))
                hint = null;

            var name = TryResolveFileName(hint);
            if (name != null && IsPlaceholderFileName(name))
                name = null;

            if (name == null && !string.IsNullOrWhiteSpace(productName))
                name = TryResolveFileName(ExtractLeadingNumericCode(productName));

            if (name == null)
                return null;
            return "~/images/products/" + Uri.EscapeDataString(name);
        }

        private static string? StripToFileName(string? photoFromDb)
        {
            if (string.IsNullOrWhiteSpace(photoFromDb))
                return null;
            var raw = photoFromDb.Trim().Replace('\\', '/');
            if (raw.StartsWith("/images/products/", StringComparison.OrdinalIgnoreCase))
                raw = raw["/images/products/".Length..];
            else if (raw.StartsWith("images/products/", StringComparison.OrdinalIgnoreCase))
                raw = raw["images/products/".Length..];
            else if (raw.StartsWith("products/", StringComparison.OrdinalIgnoreCase))
                raw = raw["products/".Length..];
            var n = Path.GetFileName(raw);
            return string.IsNullOrEmpty(n) ? null : n;
        }

        private static bool IsPlaceholderFileName(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
            var n = Path.GetFileName(fileName).ToLowerInvariant();
            return n is "nophoto.png" or "nophoto.jpg" or "demo.png";
        }

        private static string? ExtractLeadingNumericCode(string productName)
        {
            var dash = productName.IndexOf('-');
            if (dash <= 0)
                return null;
            var code = productName[..dash].Trim();
            if (code.Length == 0)
                return null;
            foreach (var c in code)
            {
                if (c < '0' || c > '9')
                    return null;
            }
            return code;
        }

        private string? TryResolveFileName(string? hint)
        {
            if (string.IsNullOrWhiteSpace(hint))
                return null;

            var raw = hint.Trim().Replace('\\', '/');
            if (raw.StartsWith("/images/products/", StringComparison.OrdinalIgnoreCase))
                raw = raw["/images/products/".Length..];
            else if (raw.StartsWith("images/products/", StringComparison.OrdinalIgnoreCase))
                raw = raw["images/products/".Length..];
            else if (raw.StartsWith("products/", StringComparison.OrdinalIgnoreCase))
                raw = raw["products/".Length..];

            var name = Path.GetFileName(raw);
            if (string.IsNullOrEmpty(name))
                return null;

            if (!Directory.Exists(_dir))
                return null;

            if (File.Exists(Path.Combine(_dir, name)))
                return name;

            var baseName = Path.GetFileNameWithoutExtension(name);

            foreach (var ext in Extensions)
            {
                var candidate = baseName + ext;
                if (File.Exists(Path.Combine(_dir, candidate)))
                    return candidate;
            }

            return null;
        }
    }
}
