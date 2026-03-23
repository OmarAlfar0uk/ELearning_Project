using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class BatchConfiguration : IEntityTypeConfiguration<Batch>
    {
        public void Configure(EntityTypeBuilder<Batch> builder)
        {
            builder.ToTable("Batches");

            builder.Property(b => b.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasMany(b => b.Tracks)
                   .WithOne(t => t.Batch)
                   .HasForeignKey(t => t.BatchId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
