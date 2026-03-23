using Auth.Models;
using ELearningProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Data
{
    public class UniversitySystemAuthContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public UniversitySystemAuthContext(DbContextOptions<UniversitySystemAuthContext> options)
            : base(options)
        {
        }
      
        public DbSet<ActivationCode> ActivationCodes => Set<ActivationCode>();
        public DbSet<Batch> Batches => Set<Batch>();
        public DbSet<BatchStudent> BatchStudents => Set<BatchStudent>();
        public DbSet<Track> Tracks => Set<Track>();
        public DbSet<Lecture> Lectures => Set<Lecture>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<Progress> Progresses => Set<Progress>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<InstructorTrack> InstructorTracks => Set<InstructorTrack>();
        public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
        public DbSet<Article> Articles => Set<Article>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(UniversitySystemAuthContext).Assembly);

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            modelBuilder.Entity<InstructorTrack>()
                .HasIndex(it => new { it.InstructorId, it.TrackId }).IsUnique();

            modelBuilder.Entity<InstructorTrack>()
                .HasOne(it => it.Instructor)
                .WithMany(u => u.InstructorTracks)
                .HasForeignKey(it => it.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InstructorTrack>()
                .HasOne(it => it.Track)
                .WithMany(t => t.InstructorTracks)
                .HasForeignKey(it => it.TrackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global Query Filters for Soft Delete
            modelBuilder.Entity<ActivationCode>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Batch>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<BatchStudent>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Track>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Lecture>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Assignment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Submission>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Progress>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Article>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<InstructorTrack>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<UploadedFile>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
