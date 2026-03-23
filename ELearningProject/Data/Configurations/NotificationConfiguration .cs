using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.Property(n => n.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(n => n.Message)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.HasOne(n => n.User)
                   .WithMany()
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
