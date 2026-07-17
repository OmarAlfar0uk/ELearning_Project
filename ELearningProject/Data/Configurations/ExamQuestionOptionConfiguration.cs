using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// Configuration for the ExamQuestionOption entity mapping.
    /// </summary>
    public class ExamQuestionOptionConfiguration : IEntityTypeConfiguration<ExamQuestionOption>
    {
        public void Configure(EntityTypeBuilder<ExamQuestionOption> builder)
        {
            builder.ToTable("ExamQuestionOptions");

            builder.Property(o => o.Text)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.HasOne(o => o.ExamQuestion)
                   .WithMany(q => q.Options)
                   .HasForeignKey(o => o.ExamQuestionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
