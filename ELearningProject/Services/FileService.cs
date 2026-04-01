
using ELearningProject.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ELearningProject.Data.UniversitySystemAuthContext _context;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".zip", ".png", ".jpg", ".jpeg" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public FileService(
            IWebHostEnvironment env, 
            IHttpContextAccessor httpContextAccessor,
            ELearningProject.Data.UniversitySystemAuthContext context)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName, Guid? userId = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            if (file.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds the limit of {_maxFileSize / 1024 / 1024}MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"File type '{extension}' is not allowed.");

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadPath = Path.Combine(webRootPath, "Uploads", folderName);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var storedName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, storedName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = Path.Combine("Uploads", folderName, storedName).Replace("\\", "/");
            var fullUrl = BuildFullUrl(relativeUrl);

            // Save to Database
            var uploadedFile = new ELearningProject.Models.UploadedFile
            {
                FileName = file.FileName,
                StoredName = storedName,
                FileUrl = fullUrl,
                ContentType = file.ContentType,
                Size = file.Length,
                Folder = folderName,
                UploadedById = userId
            };

            _context.UploadedFiles.Add(uploadedFile);
            await _context.SaveChangesAsync();

            return fullUrl;
        }

        public async Task<List<string>> SaveFilesBulkAsync(List<IFormFile> files, string folderName, Guid? userId = null)
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                urls.Add(await SaveFileAsync(file, folderName, userId));
            }
            return urls;
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return false;

            var uploadsIndex = fileUrl.IndexOf("Uploads", StringComparison.OrdinalIgnoreCase);
            if (uploadsIndex == -1) return false;

            var relativePath = fileUrl.Substring(uploadsIndex);
            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRootPath, relativePath);

            // Soft remove from DB
            var fileRecord = await _context.UploadedFiles.FirstOrDefaultAsync(f => f.FileUrl == fileUrl && !f.IsDeleted);
            if (fileRecord != null)
            {
                fileRecord.IsDeleted = true;
                fileRecord.UpdatedAt = DateTime.UtcNow;
                _context.UploadedFiles.Update(fileRecord);
                await _context.SaveChangesAsync();
            }

            return fileRecord != null;
        }

        public async Task<ELearningProject.Models.UploadedFile?> GetFileByIdAsync(Guid id)
        {
            var file = await _context.UploadedFiles.FindAsync(id);
            return file != null && !file.IsDeleted ? file : null;
        }

        public async Task<List<ELearningProject.Models.UploadedFile>> GetAllFilesAsync()
        {
            return await _context.UploadedFiles
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateFileMetadataAsync(Guid id, string newFileName)
        {
            var file = await _context.UploadedFiles.FindAsync(id);
            if (file == null) return false;

            file.FileName = newFileName;
            file.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }

        private string BuildFullUrl(string relativeUrl)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                return $"{baseUrl}/{relativeUrl}";
            }
            return "/" + relativeUrl;
        }
    }
}
