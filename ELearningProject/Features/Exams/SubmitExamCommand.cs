using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Command to submit the current student's answers for an exam attempt.
    /// </summary>
    public record SubmitExamCommand(
        Guid AttemptId,
        List<SubmitAnswerRequest> Answers
    ) : IRequest<EndpointResponse<ExamAttemptDto>>;
}
