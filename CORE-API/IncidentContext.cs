using Microsoft.EntityFrameworkCore;

namespace CORE_API
{
    public class IncidentContext : DbContext
    {
        public DbSet<Incident> Incidents { get; set; }

        public IncidentContext(DbContextOptions<IncidentContext> options) : base(options) { }

    }
}
