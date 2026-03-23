
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Lectures.DTOs
{
    public record UpdateLectureRequest(
        string Title,
        string? ContentText,
        string? DriveLink,
        IFormFile? File
    );
}
