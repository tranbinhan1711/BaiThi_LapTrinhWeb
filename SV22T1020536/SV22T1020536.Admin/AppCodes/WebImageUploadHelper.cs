using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SV22T1020536.Admin.AppCodes;

/// <summary>
/// Lưu file ảnh upload vào wwwroot (sản phẩm / nhân viên).
/// </summary>
public static class WebImageUploadHelper
{
    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp"
    };

    /// <summary>
    /// Lưu vào wwwroot/images/products, trả về tên file để ghi DB (ví dụ p-abc....jpg).
    /// </summary>
    public static async Task<string?> SaveProductImageAsync(
        IWebHostEnvironment env,
        IFormFile? file,
        ModelStateDictionary modelState,
        string modelErrorKey)
    {
        return await SaveAsync(env, file, modelState, modelErrorKey, "images", "products", "p");
    }

    /// <summary>
    /// Lưu ảnh thư viện (album) vào wwwroot/images/products, tên file tiền tố g-.
    /// </summary>
    public static async Task<string?> SaveProductGalleryImageAsync(
        IWebHostEnvironment env,
        IFormFile? file,
        ModelStateDictionary modelState,
        string modelErrorKey)
    {
        return await SaveAsync(env, file, modelState, modelErrorKey, "images", "products", "g");
    }

    /// <summary>
    /// Lưu vào wwwroot/images/employees, trả về tên file để ghi DB.
    /// </summary>
    public static async Task<string?> SaveEmployeeImageAsync(
        IWebHostEnvironment env,
        IFormFile? file,
        ModelStateDictionary modelState,
        string modelErrorKey)
    {
        return await SaveAsync(env, file, modelState, modelErrorKey, "images", "employees", "e");
    }

    private static async Task<string?> SaveAsync(
        IWebHostEnvironment env,
        IFormFile? file,
        ModelStateDictionary modelState,
        string modelErrorKey,
        string segment1,
        string segment2,
        string prefix)
    {
        if (file == null || file.Length == 0)
            return null;

        if (file.Length > MaxBytes)
        {
            modelState.AddModelError(modelErrorKey, "Ảnh không được vượt quá 5 MB.");
            return null;
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            modelState.AddModelError(modelErrorKey, "Chỉ chấp nhận file ảnh: JPG, PNG, GIF, WEBP, BMP.");
            return null;
        }

        if (!string.IsNullOrEmpty(file.ContentType)
            && file.ContentType != "application/octet-stream"
            && !AllowedContentTypes.Contains(file.ContentType))
        {
            modelState.AddModelError(modelErrorKey, "Định dạng file không hợp lệ.");
            return null;
        }

        var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(root, segment1, segment2);
        Directory.CreateDirectory(dir);

        var safeExt = ext.ToLowerInvariant();
        var fileName = $"{prefix}-{Guid.NewGuid():N}{safeExt}";
        var fullPath = Path.Combine(dir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }
}
