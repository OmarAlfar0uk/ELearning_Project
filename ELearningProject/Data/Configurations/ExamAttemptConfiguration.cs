using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// Configuration for the ExamAttempt entity mapping.
    /// </summary>
    public class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
    {
        public void Configure(EntityTypeBuilder<ExamAttempt> builder)
        {
            builder.ToTable("ExamAttempts");

            builder.HasIndex(attempt => new { attempt.ExamId, attempt.StudentId })
                   .IsUnique();

            builder.HasOne(attempt => attempt.Exam)
                   .WithMany()
                   .HasForeignKey(attempt => attempt.ExamId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(attempt => attempt.Student)
                   .WithMany()
                   .HasForeignKey(attempt => attempt.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
