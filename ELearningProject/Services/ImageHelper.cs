using Auth.Contarcts;

namespace Auth.Services
{
    public class ImageHelper : IImageHelper
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".webp"
        };

        #region DI
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        public ImageHelper(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool DeleteImage(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var normalizedPath = NormalizeImagePath(relativePath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                return false;

            var fullPath = Path.Combine(webRootPath, normalizedPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }

        public string? GetImageUrl(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            if (Uri.IsWellFormedUriString(relativePath, UriKind.Absolute))
                return relativePath;

            var normalizedPath = NormalizeImagePath(relativePath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return "/" + normalizedPath;

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}/{normalizedPath}";
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string subFolder)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Image file is required.");

            var fileExtension = Path.GetExtension(imageFile.FileName);
            if (!AllowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Only PNG, JPG, JPEG, and WEBP images are allowed.");

            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRootPath, "Uploads", "Images", subFolder);

            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return Path.Combine("Uploads", "Images", subFolder, fileName).Replace("\\", "/");
        }

        private static string? NormalizeImagePath(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            if (Uri.TryCreate(imagePath, UriKind.Absolute, out var uri))
                imagePath = uri.LocalPath;

            var uploadsIndex = imagePath.IndexOf("Uploads", StringComparison.OrdinalIgnoreCase);
            if (uploadsIndex >= 0)
                imagePath = imagePath[uploadsIndex..];

            return imagePath.Replace("\\", "/").TrimStart('/');
        }
    }
}
