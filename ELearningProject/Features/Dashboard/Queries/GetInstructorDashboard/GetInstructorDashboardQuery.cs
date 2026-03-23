
using ELearningProject.Features.Dashboard.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Dashboard.Queries.GetInstructorDashboard
{
    public record GetInstructorDashboardQuery(Guid InstructorId) : IRequest<EndpointResponse<InstructorDashboardDto>>;
}
