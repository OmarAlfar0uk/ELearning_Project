using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class LectureConfiguration : IEntityTypeConfiguration<Lecture>
    {
        public void Configure(EntityTypeBuilder<Lecture> builder)
        {
            builder.ToTable("Lectures");

            builder.Property(l => l.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasOne(l => l.Track)
                   .WithMany(t => t.Lectures)
                   .HasForeignKey(l => l.TrackId);

            builder.HasMany(l => l.Assignments)
                   .WithOne(a => a.Lecture)
                   .HasForeignKey(a => a.LectureId);
        }
    }
}
