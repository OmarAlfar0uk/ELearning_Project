
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Assignments.CreateAssignment
{
    public record CreateAssignmentRequest(
        string Title,
        int MaxScore,
        DateTime? DueDate,
        IFormFile? File
    );
}
