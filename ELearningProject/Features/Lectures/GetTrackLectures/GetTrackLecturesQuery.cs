
using ELearningProject.Features.Lectures.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Lectures.GetTrackLectures
{
    public record GetTrackLecturesQuery(Guid TrackId) : IRequest<EndpointResponse<List<LectureDto>>>;
}
