using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.ToTable("Articles");

            builder.Property(a => a.Title)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(a => a.Content)
                   .IsRequired();

            builder.Property(a => a.ImageUrl)
                   .HasMaxLength(500);

            builder.HasOne(a => a.Author)
                   .WithMany()
                   .HasForeignKey(a => a.AuthorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
