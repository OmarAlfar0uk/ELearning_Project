namespace ELearningProject.Models
{
    /// <summary>
    /// Identifies the originating trigger that produced a <see cref="CoinTransaction"/>.
    /// </summary>
    public enum CoinSource
    {
        /// <summary>Coins granted manually by an Admin or Instructor.</summary>
        AdminGrant = 0,

        /// <summary>Coins awarded automatically when a student passes an exam (Stage 2+).</summary>
        ExamPassed = 1,

        /// <summary>Coins awarded automatically when an assignment is submitted on time (Stage 2+).</summary>
        AssignmentOnTime = 2
    }
}
