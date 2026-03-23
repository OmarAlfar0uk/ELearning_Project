
using ELearningProject.Features.Lectures.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Lectures.GetLectureDetails
{
    public record GetLectureDetailsQuery(Guid LectureId) : IRequest<EndpointResponse<LectureDetailsDto>>;
}
