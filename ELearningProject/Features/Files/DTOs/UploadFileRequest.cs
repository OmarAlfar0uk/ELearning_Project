using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Files.DTOs
{
    public record UploadFileRequest(
        IFormFile File,
        string? Folder
    );
}
