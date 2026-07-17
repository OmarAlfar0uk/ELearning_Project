using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// Configuration for the ExamQuestion entity mapping.
    /// </summary>
    public class ExamQuestionConfiguration : IEntityTypeConfiguration<ExamQuestion>
    {
        public void Configure(EntityTypeBuilder<ExamQuestion> builder)
        {
            builder.ToTable("ExamQuestions");

            builder.Property(q => q.Text)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.HasOne(q => q.Exam)
                   .WithMany(e => e.Questions)
                   .HasForeignKey(q => q.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
