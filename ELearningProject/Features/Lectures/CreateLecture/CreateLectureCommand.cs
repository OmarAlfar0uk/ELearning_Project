
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Lectures.CreateLecture
{
    public record CreateLectureCommand(
        Guid TrackId,
        string Title,
        string? ContentText,
        string? DriveLink,
        string? FileUrl
    ) : IRequest<EndpointResponse<Guid>>;
}
