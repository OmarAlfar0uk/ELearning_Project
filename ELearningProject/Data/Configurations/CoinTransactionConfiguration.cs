using Auth.Models;
using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearningProject.Data.Configurations
{
    /// <summary>
    /// EF Core fluent configuration for the <see cref="CoinTransaction"/> entity.
    /// Defines the table name, column constraints, and both foreign-key relationships
    /// to <see cref="ApplicationUser"/> (as Student and as GrantedByAdmin).
    /// </summary>
    public class CoinTransactionConfiguration : IEntityTypeConfiguration<CoinTransaction>
    {
        public void Configure(EntityTypeBuilder<CoinTransaction> builder)
        {
            builder.ToTable("CoinTransactions");

            // ── Scalar columns ──────────────────────────────────────────────────────
            builder.Property(ct => ct.Amount)
                   .IsRequired();

            builder.Property(ct => ct.Reason)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(ct => ct.Source)
                   .IsRequired()
                   .HasConversion<int>();

            // ── Relationships ────────────────────────────────────────────────────────

            // M:1 → ApplicationUser (Student, required)
            builder.HasOne(ct => ct.Student)
                   .WithMany(u => u.CoinTransactions)
                   .HasForeignKey(ct => ct.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // M:1 → ApplicationUser (GrantedByAdmin, optional)
            builder.HasOne(ct => ct.GrantedByAdmin)
                   .WithMany()
                   .HasForeignKey(ct => ct.GrantedByAdminId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
