using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
/*using Conduit.Domain.Common;
using Conduit.Domain.User;
using Conduit.Persistence.ContextConfiguration;*/
using Microsoft.EntityFrameworkCore;

namespace Conduit.UsersManagement.Persistence;

public class UsersManagementContext : DbContext
{
    /*public DbSet<User> Users { get; set; } = null!;*/

    public UsersManagementContext()
    {
        // for ef tools
    }

    public UsersManagementContext(DbContextOptions<UsersManagementContext> options) : base(options)
    {
    }

    /*protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql($"Data Source=db/conduit.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserEntityTypeConfiguration).Assembly);
    }*/
}
