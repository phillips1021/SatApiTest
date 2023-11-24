using Microsoft.EntityFrameworkCore;

namespace SatApiTest.Models
{
    public class SatContext : DbContext
    {

        public SatContext(DbContextOptions<SatContext> options)
            : base(options)
        {
        }

        public DbSet<SelfAssessment> SelfAssessments { get; set; } = null!;


    }
}
