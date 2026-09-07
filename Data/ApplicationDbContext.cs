using Microsoft.EntityFrameworkCore;
using StudentServiceRequestSystem.Models;

namespace StudentServiceRequestSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }
    }
}