
namespace ELearningProject.Features.Progress.DTOs
{
    public record ProgressDto(
        Guid StudentId,
        Guid TrackId,
        int CompletionPercentage,
        int Rank,
        double AverageScore
    );
}
