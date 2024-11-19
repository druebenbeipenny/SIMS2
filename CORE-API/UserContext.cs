using Microsoft.EntityFrameworkCore;

namespace CORE_API
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        //public DbSet<Incident> Incidents { get; set; }

        public UserContext(DbContextOptions<UserContext> options) : base(options) { }
        

    }
}
