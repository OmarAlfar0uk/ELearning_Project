using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class BatchStudentConfiguration : IEntityTypeConfiguration<BatchStudent>
    {
        public void Configure(EntityTypeBuilder<BatchStudent> builder)
        {
            builder.ToTable("BatchStudents");

            builder.HasIndex(bs => new { bs.StudentId, bs.BatchId })
                   .IsUnique();

            builder.HasOne(bs => bs.Student)
                   .WithMany()
                   .HasForeignKey(bs => bs.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(bs => bs.Batch)
                   .WithMany(b => b.Students)
                   .HasForeignKey(bs => bs.BatchId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
