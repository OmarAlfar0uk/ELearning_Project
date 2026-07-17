using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Request model for an exam question option.
    /// </summary>
    public record CreateOptionRequest(string Text, bool IsCorrect);

    /// <summary>
    /// Command to add a question and its options to an exam.
    /// </summary>
    public record AddExamQuestionCommand(
        Guid ExamId,
        string Text,
        QuestionType Type,
        int Points,
        int Order,
        List<CreateOptionRequest> Options
    ) : IRequest<EndpointResponse<ExamQuestionDto>>;
}
