using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Command to close an exam and prevent further authoring changes.
    /// </summary>
    public record CloseExamCommand(Guid ExamId)
        : IRequest<EndpointResponse<string>>;
}
