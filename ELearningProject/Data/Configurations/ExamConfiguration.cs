using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// Configuration for the Exam entity mapping.
    /// </summary>
    public class ExamConfiguration : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> builder)
        {
            builder.ToTable("Exams");

            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasOne(e => e.Track)
                   .WithMany(t => t.Exams)
                   .HasForeignKey(e => e.TrackId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
