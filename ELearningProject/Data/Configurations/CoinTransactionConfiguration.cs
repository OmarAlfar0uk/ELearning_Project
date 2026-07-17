using Auth.Models;
using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// EF Core fluent configuration for the <see cref="CoinTransaction"/> entity.
    /// Defines the table name, column constraints, and all foreign-key relationships:
    /// <list type="bullet">
    ///   <item>M:1 → ApplicationUser as Student (required, Restrict)</item>
    ///   <item>M:1 → ApplicationUser as GrantedByAdmin (optional, Restrict)</item>
    ///   <item>M:1 → ExamAttempt as RelatedExamAttempt (optional, Restrict) — Stage 2 idempotency guard</item>
    ///   <item>M:1 → Submission as RelatedSubmission (optional, Restrict) — Stage 2 idempotency guard</item>
    /// </list>
    /// </summary>
    public class CoinTransactionConfiguration : IEntityTypeConfiguration<CoinTransaction>
    {
        public void Configure(EntityTypeBuilder<CoinTransaction> builder)
        {
            builder.ToTable("CoinTransactions");

            // ── Scalar columns ──────────────────────────────────────────────────────
            builder.Property(ct => ct.Amount)
                   .IsRequired();

            builder.Property(ct => ct.Reason)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(ct => ct.Source)
                   .IsRequired()
                   .HasConversion<int>();

            // ── Relationships ────────────────────────────────────────────────────────

            // M:1 → ApplicationUser (Student, required)
            builder.HasOne(ct => ct.Student)
                   .WithMany(u => u.CoinTransactions)
                   .HasForeignKey(ct => ct.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // M:1 → ApplicationUser (GrantedByAdmin, optional)
            builder.HasOne(ct => ct.GrantedByAdmin)
                   .WithMany()
                   .HasForeignKey(ct => ct.GrantedByAdminId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            // M:1 → ExamAttempt (optional — Stage 2 idempotency guard for ExamPassed)
            // Restrict: prevents physical deletion of an attempt that has already
            // generated a coin award, preserving audit integrity.
            builder.HasOne(ct => ct.RelatedExamAttempt)
                   .WithMany()
                   .HasForeignKey(ct => ct.RelatedExamAttemptId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            // M:1 → Submission (optional — Stage 2 idempotency guard for AssignmentOnTime)
            // Restrict: same rationale as above.
            builder.HasOne(ct => ct.RelatedSubmission)
                   .WithMany()
                   .HasForeignKey(ct => ct.RelatedSubmissionId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
