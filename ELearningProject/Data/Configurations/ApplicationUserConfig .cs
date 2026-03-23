using Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
          
            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.ProfileImageUrl)
                   .HasMaxLength(300);

            builder.Property(u => u.IsActivated)
                   .IsRequired();

            builder.Property(u => u.CreatedAt)
                   .IsRequired();

            builder.Property(u => u.LastLoginAt);

            builder.Property(u => u.DateOfBirth);

            builder.Property(u => u.Gender)
                   .HasConversion<string>()   
                   .HasMaxLength(10);

            builder.Property(u => u.RefreshToken)
                   .HasMaxLength(500);

            builder.Property(u => u.RefreshTokenExpiryTime);

            builder.Ignore(u => u.FullName);
        }
    }
}
