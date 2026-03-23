
using ELearningProject.Features.Progress.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Progress.GetStudentProgress
{
    public record GetStudentProgressQuery(Guid TrackId, Guid StudentId) : IRequest<EndpointResponse<ProgressDto>>;
}
