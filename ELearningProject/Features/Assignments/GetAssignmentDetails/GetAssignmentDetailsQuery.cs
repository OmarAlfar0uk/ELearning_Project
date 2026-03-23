
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.GetAssignmentDetails
{
    public record GetAssignmentDetailsQuery(Guid AssignmentId) : IRequest<EndpointResponse<AssignmentDetailsDto>>;
}
