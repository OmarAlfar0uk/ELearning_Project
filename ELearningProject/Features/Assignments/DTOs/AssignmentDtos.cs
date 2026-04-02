
using ELearningProject.Features.Batche.GetBatchById; // For TrackDto/LectureDto reuse if possible? No, let's create simple DTOs or reuse.
// To keep it simple and independent:

namespace ELearningProject.Features.Assignments.DTOs
{
    public record AssignmentDto(
        Guid Id,
        string Title,
        int MaxScore,
        DateTime? DueDate,
        bool IsClosed,
        string? FileUrl = null
    );

    public record AssignmentDetailsDto(
        Guid Id,
        string Title,
        int MaxScore,
        DateTime? DueDate,
        bool IsClosed,
        Guid LectureId,
        string LectureTitle,
        Guid TrackId,
        string TrackName,
        string? FileUrl = null
    );

    public record AssignmentStatsDto(
        int TotalStudents,
        int SubmittedCount,
        int NotSubmittedCount,
        double AverageScore
    );
}
