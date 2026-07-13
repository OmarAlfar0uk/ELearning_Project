using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Admin.CreateTeacher
{
    /// <summary>
    /// Command to create a new teacher (Instructor) account and link them to tracks.
    /// </summary>
    public record CreateTeacherCommand(
        string FirstName,
        string LastName,
        string Email,
        List<Guid> TrackIds
    ) : IRequest<EndpointResponse<TeacherDto>>;
}
