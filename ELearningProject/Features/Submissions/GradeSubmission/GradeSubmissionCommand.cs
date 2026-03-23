
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Submissions.GradeSubmission
{
    public record GradeSubmissionCommand(
        Guid SubmissionId,
        double Score,
        string Feedback
    ) : IRequest<RequestResponse<string>>;
}
