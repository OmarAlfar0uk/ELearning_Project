
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Lectures.UpdateLecture
{
    public record UpdateLectureCommand(
        Guid LectureId, 
        string Title,
        string? ContentText,
        string? DriveLink,
        string? FileUrl
    ) : IRequest<RequestResponse<string>>;
}
