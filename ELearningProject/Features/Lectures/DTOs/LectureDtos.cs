
namespace ELearningProject.Features.Lectures.DTOs
{
    public record LectureDto(
        Guid Id,
        string Title,
        string? ContentText,
        string? DriveLink,
        string? FileUrl,
        Guid TrackId
    );

    public record LectureDetailsDto(
        Guid Id,
        string Title,
        string? ContentText,
        string? DriveLink,
        string? FileUrl,
        Guid TrackId,
        int AssignmentCount // 0 or 1 based on current model
    );
}
