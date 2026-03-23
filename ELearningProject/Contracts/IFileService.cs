
namespace ELearningProject.Contracts
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName, Guid? userId = null);
        Task<List<string>> SaveFilesBulkAsync(List<IFormFile> files, string folderName, Guid? userId = null);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<ELearningProject.Models.UploadedFile?> GetFileByIdAsync(Guid id);
        Task<List<ELearningProject.Models.UploadedFile>> GetAllFilesAsync();
        Task<bool> UpdateFileMetadataAsync(Guid id, string newFileName);
    }
}
