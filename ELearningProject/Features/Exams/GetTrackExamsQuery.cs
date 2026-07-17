using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Query to retrieve exam summaries for a track.
    /// </summary>
    public record GetTrackExamsQuery(Guid TrackId)
        : IRequest<EndpointResponse<List<ExamDto>>>;
}
