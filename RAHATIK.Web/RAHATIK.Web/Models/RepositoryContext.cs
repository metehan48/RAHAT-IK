using Microsoft.EntityFrameworkCore;
using RAHATIK.Web.Models.Entities;

namespace RAHATIK.Web.Models
{
    public class RepositoryContext:DbContext
    {
        public DbSet<TotalPrice> TotalPrices { get; set; }

        public RepositoryContext(DbContextOptions<RepositoryContext> options)
        : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source = DESKTOP-8F4UN61\\SQLEXPRESS; Initial Catalog = RAHATIKDb; Integrated Security=true; MultipleActiveResultSets=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
