
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.GetAssignmentStats
{
    public record GetAssignmentStatsQuery(Guid AssignmentId) : IRequest<EndpointResponse<AssignmentStatsDto>>;
}
