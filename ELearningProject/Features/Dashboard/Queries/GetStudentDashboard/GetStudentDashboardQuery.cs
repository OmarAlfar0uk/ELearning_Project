
using ELearningProject.Features.Dashboard.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Dashboard.Queries.GetStudentDashboard
{
    public record GetStudentDashboardQuery(Guid StudentId) : IRequest<EndpointResponse<StudentDashboardDto>>;
}
