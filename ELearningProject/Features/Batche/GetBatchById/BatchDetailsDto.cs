namespace ELearningProject.Features.Batche.GetBatchById
{
    public record BatchDetailsDto(
        Guid Id,
        string Name,
        DateTime StartDate,
        List<BatchStudentDetailDto> Students,
        List<BatchTrackDto> Tracks
    );

    public record BatchStudentDetailDto(
        Guid StudentId,
        string FullName,
        string Email,
        int Rank,
        double AverageScore
    );

    public record BatchTrackDto(
        Guid Id,
        string Name,
        int LectureCount
    );
}
