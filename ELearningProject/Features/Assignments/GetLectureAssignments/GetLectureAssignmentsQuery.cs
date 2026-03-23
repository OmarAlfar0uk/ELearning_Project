
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.GetLectureAssignments
{
    public record GetLectureAssignmentsQuery(Guid LectureId) : IRequest<EndpointResponse<List<AssignmentDto>>>;
}
