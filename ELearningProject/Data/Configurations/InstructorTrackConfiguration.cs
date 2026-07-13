using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class InstructorTrackConfiguration : IEntityTypeConfiguration<InstructorTrack>
    {
        public void Configure(EntityTypeBuilder<InstructorTrack> builder)
        {
            builder.ToTable("InstructorTracks");

            builder.HasIndex(it => new { it.InstructorId, it.TrackId }).IsUnique();

            builder.HasOne(it => it.Instructor)
                   .WithMany(u => u.InstructorTracks)
                   .HasForeignKey(it => it.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(it => it.Track)
                   .WithMany(t => t.InstructorTracks)
                   .HasForeignKey(it => it.TrackId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(it => it.AssignedByAdmin)
                   .WithMany()
                   .HasForeignKey(it => it.AssignedByAdminId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
