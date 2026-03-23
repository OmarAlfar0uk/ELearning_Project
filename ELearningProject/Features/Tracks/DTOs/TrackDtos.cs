
namespace ELearningProject.Features.Tracks.DTOs
{
    public record TrackDto(
        Guid Id,
        string Name,
        Guid BatchId
    );

    public record TrackDetailsDto(
        Guid Id,
        string Name,
        Guid BatchId,
        int LectureCount
    );
}
