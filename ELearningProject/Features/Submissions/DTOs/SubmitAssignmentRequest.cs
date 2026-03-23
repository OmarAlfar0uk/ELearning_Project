
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Submissions.DTOs
{
    public record SubmitAssignmentRequest(
        IFormFile File
    );
}
