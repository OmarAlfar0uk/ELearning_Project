using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.ToTable("Assignments");

            builder.Property(a => a.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(a => a.MaxScore)
                   .HasDefaultValue(10);

            builder.HasMany(a => a.Submissions)
                   .WithOne(s => s.Assignment)
                   .HasForeignKey(s => s.AssignmentId);

        }
    }
}
