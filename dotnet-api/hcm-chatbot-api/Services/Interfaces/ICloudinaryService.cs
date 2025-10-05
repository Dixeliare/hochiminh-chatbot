using Microsoft.AspNetCore.Http;

namespace Services.Interfaces;

/// <summary>
/// Interface cho Cloudinary service - Upload/Delete ảnh
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload ảnh lên Cloudinary
    /// </summary>
    /// <param name="file">File ảnh cần upload</param>
    /// <param name="folder">Folder trên Cloudinary (default: avatars)</param>
    /// <returns>URL của ảnh đã upload</returns>
    Task<string> UploadImageAsync(IFormFile file, string folder = "avatars");

    /// <summary>
    /// Xóa ảnh trên Cloudinary
    /// </summary>
    /// <param name="publicId">Public ID của ảnh (lấy từ URL)</param>
    /// <returns>True nếu xóa thành công</returns>
    Task<bool> DeleteImageAsync(string publicId);

    /// <summary>
    /// Lấy Public ID từ Cloudinary URL
    /// </summary>
    /// <param name="imageUrl">URL của ảnh trên Cloudinary</param>
    /// <returns>Public ID</returns>
    string GetPublicIdFromUrl(string imageUrl);
}
