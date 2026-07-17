using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Records a single coin credit posted to a student's ledger.
    /// Each row represents one discrete grant — manual (Stage 1) or automatic (Stage 2+).
    /// </summary>
    public class CoinTransaction : BaseEntity
    {
        /// <summary>The student who receives the coins.</summary>
        public Guid StudentId { get; set; }

        /// <summary>Navigation property to the student's <see cref="ApplicationUser"/> record.</summary>
        public ApplicationUser Student { get; set; } = default!;

        /// <summary>
        /// Number of coins credited. Always positive.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>Human-readable explanation of why the coins were granted.</summary>
        public string Reason { get; set; } = default!;

        /// <summary>Indicates the event or action that triggered this transaction.</summary>
        public CoinSource Source { get; set; }

        /// <summary>
        /// The Admin or Instructor who manually approved this grant.
        /// Null for system-generated transactions (Stage 2+).
        /// </summary>
        public Guid? GrantedByAdminId { get; set; }

        /// <summary>
        /// Navigation property to the granting admin/instructor's <see cref="ApplicationUser"/> record.
        /// </summary>
        public ApplicationUser? GrantedByAdmin { get; set; }

        // ── Stage 2: idempotency guard columns ───────────────────────────────────
        // Exactly one of these will be non-null for automatic triggers so the system
        // can assert "no CoinTransaction already references this attempt/submission"
        // before creating a duplicate award.

        /// <summary>
        /// The <see cref="ExamAttempt"/> that triggered this transaction
        /// (set when <see cref="Source"/> == <see cref="CoinSource.ExamPassed"/>).
        /// Used as an idempotency guard — at most one transaction per attempt.
        /// </summary>
        public Guid? RelatedExamAttemptId { get; set; }

        /// <summary>Navigation property to the triggering <see cref="ExamAttempt"/>.</summary>
        public ExamAttempt? RelatedExamAttempt { get; set; }

        /// <summary>
        /// The <see cref="Submission"/> that triggered this transaction
        /// (set when <see cref="Source"/> == <see cref="CoinSource.AssignmentOnTime"/>).
        /// Used as an idempotency guard — at most one transaction per submission.
        /// </summary>
        public Guid? RelatedSubmissionId { get; set; }

        /// <summary>Navigation property to the triggering <see cref="Submission"/>.</summary>
        public Submission? RelatedSubmission { get; set; }
    }
}
