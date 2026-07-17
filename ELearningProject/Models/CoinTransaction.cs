using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Records a single coin credit posted to a student's ledger.
    /// Each row represents one discrete grant (manual or, in future stages, automatic).
    /// </summary>
    public class CoinTransaction : BaseEntity
    {
        /// <summary>The student who receives the coins.</summary>
        public Guid StudentId { get; set; }

        /// <summary>Navigation property to the student's <see cref="ApplicationUser"/> record.</summary>
        public ApplicationUser Student { get; set; } = default!;

        /// <summary>
        /// Number of coins credited. Must be positive in Stage 1 (manual grants only).
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
    }
}
