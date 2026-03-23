namespace ELearningProject.Features.Batche.GetBatchStudentStats
{
    public record BatchStudentStatsDto(
        Guid StudentId,
        string FullName,
        string Email,
        string BatchName,
        int Rank,
        double AverageScore,
        int SubmittedAssignmentsCount,
        string? ProfileImageUrl
    );
}
