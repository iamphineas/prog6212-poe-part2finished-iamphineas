using Microsoft.EntityFrameworkCore;
using ClaimPro.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ClaimPro.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Claim> Claims { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}
