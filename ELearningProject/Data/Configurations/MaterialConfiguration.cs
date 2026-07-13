using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// Configuration for the Material entity mapping.
    /// </summary>
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.ToTable("Materials");

            builder.Property(m => m.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasOne(m => m.Lecture)
                   .WithMany(l => l.Materials)
                   .HasForeignKey(m => m.LectureId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
