
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Tracks.RemoveInstructor
{
    public record RemoveInstructorCommand(Guid TrackId, Guid InstructorId) : IRequest<RequestResponse<string>>;
}
