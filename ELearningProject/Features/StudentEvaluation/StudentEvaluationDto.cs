namespace ELearningProject.Features.StudentEvaluation
{
    /// <summary>
    /// Data transfer object containing aggregate evaluation and performance metrics for a student.
    /// </summary>
    /// <param name="StudentId">Unique identifier of the student.</param>
    /// <param name="StudentName">Full name of the student.</param>
    /// <param name="OverallCompletionPercentage">Average completion percentage across enrolled tracks.</param>
    /// <param name="ExamsTaken">Count of non-in-progress exam attempts.</param>
    /// <param name="ExamsPassed">Count of exam attempts with score ≥ 60% of total points.</param>
    /// <param name="AverageExamScorePercentage">Average score percentage across graded exams.</param>
    /// <param name="AssignmentsSubmitted">Total count of submissions made by the student.</param>
    /// <param name="AssignmentsOnTime">Count of submissions submitted on or before assignment due date.</param>
    /// <param name="AverageSubmissionScore">Average score achieved across graded submissions.</param>
    /// <param name="CoinBalance">Current global coin balance of the student.</param>
    /// <param name="Rank">The student's rank in their current/latest batch (nullable if not in a batch).</param>
    /// <param name="BatchAverageScore">The student's average score across their current/latest batch (nullable).</param>
    public record StudentEvaluationDto(
        Guid StudentId,
        string StudentName,
        double OverallCompletionPercentage,
        int ExamsTaken,
        int ExamsPassed,
        double AverageExamScorePercentage,
        int AssignmentsSubmitted,
        int AssignmentsOnTime,
        double AverageSubmissionScore,
        int CoinBalance,
        int? Rank,
        double? BatchAverageScore
    );
}
