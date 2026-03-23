using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class TrackConfiguration : IEntityTypeConfiguration<Track>
    {
        public void Configure(EntityTypeBuilder<Track> builder)
        {
            builder.ToTable("Tracks");

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(t => t.Batch)
                   .WithMany(b => b.Tracks)
                   .HasForeignKey(t => t.BatchId);
        }
    }

}
