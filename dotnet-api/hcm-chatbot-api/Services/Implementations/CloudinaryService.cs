using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services.Implementations;

/// <summary>
/// Service xử lý upload/delete ảnh trên Cloudinary
/// </summary>
public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        // Lấy config từ appsettings.json
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        // Khởi tạo Cloudinary account
        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    /// <summary>
    /// Upload ảnh lên Cloudinary
    /// </summary>
    public async Task<string> UploadImageAsync(IFormFile file, string folder = "avatars")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File không hợp lệ");

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("Chỉ chấp nhận file ảnh (jpg, png, gif, webp)");

        // Validate file size (5MB)
        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File không được vượt quá 5MB");

        try
        {
            // Upload lên Cloudinary
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder, // Folder: avatars/
                Transformation = new Transformation()
                    .Width(500)
                    .Height(500)
                    .Crop("fill")
                    .Gravity("face"), // Tự động crop theo khuôn mặt
                PublicId = $"{folder}/{Guid.NewGuid()}" // Tạo unique ID
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error?.Message}");

            // Return secure URL (HTTPS)
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error uploading to Cloudinary: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Xóa ảnh trên Cloudinary
    /// </summary>
    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
            return false;

        try
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Lấy Public ID từ Cloudinary URL
    /// VD: https://res.cloudinary.com/dqvoojqdy/image/upload/v1234/avatars/abc.jpg
    /// => avatars/abc
    /// </summary>
    public string GetPublicIdFromUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return string.Empty;

        try
        {
            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');

            // Tìm index của "upload" trong URL
            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
                return string.Empty;

            // Lấy phần sau "upload/v{version}/"
            var pathSegments = segments.Skip(uploadIndex + 2).ToArray();
            var publicIdWithExtension = string.Join("/", pathSegments);

            // Bỏ extension (.jpg, .png,...)
            var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
            if (lastDotIndex > 0)
                return publicIdWithExtension.Substring(0, lastDotIndex);

            return publicIdWithExtension;
        }
        catch
        {
            return string.Empty;
        }
    }
}
