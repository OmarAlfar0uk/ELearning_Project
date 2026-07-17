using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// Configuration for the ExamAnswer entity mapping.
    /// </summary>
    public class ExamAnswerConfiguration : IEntityTypeConfiguration<ExamAnswer>
    {
        public void Configure(EntityTypeBuilder<ExamAnswer> builder)
        {
            builder.ToTable("ExamAnswers");

            builder.Property(answer => answer.AnswerText)
                   .HasMaxLength(4000);

            builder.Property(answer => answer.Feedback)
                   .HasMaxLength(4000);

            builder.HasOne(answer => answer.ExamAttempt)
                   .WithMany(attempt => attempt.Answers)
                   .HasForeignKey(answer => answer.ExamAttemptId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(answer => answer.ExamQuestion)
                   .WithMany()
                   .HasForeignKey(answer => answer.ExamQuestionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
