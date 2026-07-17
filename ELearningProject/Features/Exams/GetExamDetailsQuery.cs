using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Query to retrieve an exam with its questions and options.
    /// </summary>
    public record GetExamDetailsQuery(Guid ExamId)
        : IRequest<EndpointResponse<ExamDetailsDto>>;
}
