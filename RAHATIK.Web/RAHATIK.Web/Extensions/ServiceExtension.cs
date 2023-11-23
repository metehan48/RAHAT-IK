using Microsoft.EntityFrameworkCore;
using RAHATIK.Web.Models;

namespace RAHATIK.Web.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureDbContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("mssqlconnection"),
                    b => b.MigrationsAssembly("RAHATIK.Web"));
                options.EnableSensitiveDataLogging(true);
            });
        }
    }
}
