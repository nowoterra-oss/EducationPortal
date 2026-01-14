using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EduPortal.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Önce environment variable'dan connection string'i al (CI/CD için)
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ConnectionString");

            // Debug log
            Console.WriteLine("=======================================================");
            Console.WriteLine($"[DB CONTEXT FACTORY] Checking environment variable...");
            Console.WriteLine($"[DB CONTEXT FACTORY] ConnectionStrings__ConnectionString: {(string.IsNullOrEmpty(connectionString) ? "(not set)" : "found")}");
            Console.WriteLine("=======================================================");

            // Environment variable yoksa appsettings'den oku
            if (string.IsNullOrEmpty(connectionString))
            {
                var currentDir = Directory.GetCurrentDirectory();
                var basePath = FindApiProjectPath(currentDir);
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

                Console.WriteLine($"[DB CONTEXT FACTORY] Falling back to appsettings...");
                Console.WriteLine($"[DB CONTEXT FACTORY] Current Directory: {currentDir}");
                Console.WriteLine($"[DB CONTEXT FACTORY] Base Path: {basePath}");
                Console.WriteLine($"[DB CONTEXT FACTORY] Environment: {environment}");

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                connectionString = configuration.GetConnectionString("ConnectionString");
            }

            Console.WriteLine("=======================================================");
            Console.WriteLine($"[CONNECTION STRING] Using database connection:");
            Console.WriteLine($"[CONNECTION STRING] {(string.IsNullOrEmpty(connectionString) ? "(empty)" : "configured")}");
            Console.WriteLine("=======================================================");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private static string FindApiProjectPath(string currentDir)
        {
            // Eğer zaten EduPortal.API klasöründeysek
            if (currentDir.EndsWith("EduPortal.API"))
                return currentDir;

            // EduPortal.Infrastructure klasöründeysek
            if (currentDir.EndsWith("EduPortal.Infrastructure"))
                return Path.GetFullPath(Path.Combine(currentDir, "..", "EduPortal.API"));

            // Solution root'ta src/EduPortal.API ara
            var srcApiPath = Path.Combine(currentDir, "src", "EduPortal.API");
            if (Directory.Exists(srcApiPath))
                return srcApiPath;

            // Fallback
            return Path.GetFullPath(Path.Combine(currentDir, "..", "EduPortal.API"));
        }
    }
}
