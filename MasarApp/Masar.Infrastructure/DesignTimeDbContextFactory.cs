using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Masar.Infrastructure;

/// <summary>
/// Design-time DbContext factory for EF Core tools (migrations, etc.)
/// This bypasses the scoped service issue with AuditInterceptor during design-time operations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MasarDbContext>
{
    public MasarDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasarDbContext>();
        optionsBuilder.UseOracle(@"User Id=masar;Password=masar;Data Source=192.168.56.101:1521/FREEPDB1");
        return new MasarDbContext(optionsBuilder.Options);
    }
}
