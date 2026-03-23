using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ELearningProject.Data
{
    public class UniversitySystemAuthContextFactory : IDesignTimeDbContextFactory<UniversitySystemAuthContext>
    {
        public UniversitySystemAuthContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // path للـ project root
                .AddJsonFile("appsettings.json", optional: false)// جلب الـ connection string
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UniversitySystemAuthContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            return new UniversitySystemAuthContext(optionsBuilder.Options);
        }
    }
}
