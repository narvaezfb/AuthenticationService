using Authentication_Service.Models;
using Microsoft.EntityFrameworkCore;

public class AuthenticationServiceDbContext : DbContext
{
    public AuthenticationServiceDbContext(DbContextOptions<AuthenticationServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply entity configurations from separate configuration classes
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        // Other configurations
    }
}
