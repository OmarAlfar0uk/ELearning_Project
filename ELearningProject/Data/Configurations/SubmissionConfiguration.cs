using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            builder.ToTable("Submissions");

            builder.Property(s => s.FileUrl)
                   .IsRequired();

            builder.HasIndex(s => new { s.AssignmentId, s.StudentId })
                   .IsUnique();

            builder.HasOne(s => s.Student)
                   .WithMany()
                   .HasForeignKey(s => s.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
