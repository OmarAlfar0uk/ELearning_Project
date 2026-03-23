
using ELearningProject.Features.Dashboard.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Dashboard.Queries.GetAdminDashboard
{
    public record GetAdminDashboardQuery() : IRequest<EndpointResponse<AdminDashboardDto>>;
}
