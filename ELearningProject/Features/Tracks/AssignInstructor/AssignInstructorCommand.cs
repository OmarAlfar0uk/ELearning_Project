
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Tracks.AssignInstructor
{
    public record AssignInstructorCommand(Guid TrackId, Guid InstructorId) : IRequest<RequestResponse<string>>;
}
