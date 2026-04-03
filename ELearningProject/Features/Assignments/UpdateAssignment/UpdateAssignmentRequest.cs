using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Assignments.UpdateAssignment
{
    public record UpdateAssignmentRequest(
        string Title,
        int MaxScore,
        DateTime? DueDate,
        IFormFile? File
    );
}
