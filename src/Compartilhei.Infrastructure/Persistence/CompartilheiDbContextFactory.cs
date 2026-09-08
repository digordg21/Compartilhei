using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Compartilhei.Infrastructure.Persistence;

public sealed class CompartilheiDbContextFactory
    : IDesignTimeDbContextFactory<CompartilheiDbContext>
{
    public CompartilheiDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            "Server=(localdb)\\MSSQLLocalDB;" +
            "Database=Compartilhei;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        var optionsBuilder =
            new DbContextOptionsBuilder<CompartilheiDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new CompartilheiDbContext(optionsBuilder.Options);
    }
}