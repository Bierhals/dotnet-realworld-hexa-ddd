using Conduit.Domain.User;
using Conduit.Persistence.ContextConfiguration;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Persistence;

public class SqliteContext : DbContext
{
    public DbSet<User>? Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source=conduit.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserEntityTypeConfiguration).Assembly);
    }
}
