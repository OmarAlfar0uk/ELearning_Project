
namespace ELearningProject.Features.Submissions.DTOs
{
    public record SubmissionDto(
        Guid Id,
        Guid AssignmentId,
        string FileUrl,
        double? Score,
        string? Feedback,
        bool IsFinalized,
        DateTime CreatedAt
    );
}
