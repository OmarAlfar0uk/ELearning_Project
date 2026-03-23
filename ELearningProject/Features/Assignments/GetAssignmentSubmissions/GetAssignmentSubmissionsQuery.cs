
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.GetAssignmentSubmissions
{
    // Reusing AssignmentDto or creating specific SubmissionDto? 
    // User asked for: StudentName, FileUrl, Score, IsFinalized.
    // Let's create a specific DTO for this view.
    public record AssignmentSubmissionDto(
        Guid SubmissionId,
        Guid StudentId,
        string StudentName,
        string FileUrl,
        double? Score,
        bool IsFinalized,
        DateTime SubmittedAt
    );

    public record GetAssignmentSubmissionsQuery(Guid AssignmentId) : IRequest<EndpointResponse<List<AssignmentSubmissionDto>>>;
}
