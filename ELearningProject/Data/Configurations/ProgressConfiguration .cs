using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class ProgressConfiguration : IEntityTypeConfiguration<Models.Progress>
    {
        public void Configure(EntityTypeBuilder<Models.Progress> builder)
        {
            builder.ToTable("Progress");

            builder.HasIndex(p => new { p.StudentId, p.TrackId })
                   .IsUnique();

            builder.Property(p => p.CompletionPercentage)
                   .HasDefaultValue(0);
        }
    }
}
