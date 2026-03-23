namespace ELearningProject.Features.Dashboard.DTOs
{
    public record AdminDashboardDto(
        int TotalStudents,
        int TotalAdmins,
        int TotalTracks,
        int TotalBatches,
        List<TrackPerformanceDto> TopTracks,
        List<RecentUserDto> RecentUsers,
        List<TopStudentDto> TopStudents
    );

    public record TrackPerformanceDto(string TrackName, double AverageScore, int StudentCount);
    public record RecentUserDto(string FullName, string Email, string Role, DateTime CreatedAt);
    public record TopStudentDto(string StudentName, string Email, double AverageScore, string BatchName);

    public record InstructorDashboardDto(
        int MyTracksCount,
        int TotalStudents,
        int PendingSubmissions,
        List<DashboardSubmissionDto> RecentSubmissions
    );

    public record DashboardSubmissionDto(string StudentName, string AssignmentTitle, DateTime SubmittedAt, Guid SubmissionId);

    public record StudentDashboardDto(
        int EnrolledTracksCount,
        int CompletedAssignments,
        double AverageGrade,
        List<UpcomingDeadlineDto> UpcomingDeadlines,
        List<RecentGradeDto> RecentGrades
    );

    public record UpcomingDeadlineDto(string AssignmentTitle, DateTime DueDate, string CourseName);
    public record RecentGradeDto(string AssignmentTitle, double Grade, string Feedback);
}

