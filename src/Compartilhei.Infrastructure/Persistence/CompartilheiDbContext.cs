using Compartilhei.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Compartilhei.Infrastructure.Persistence;

public sealed class CompartilheiDbContext : DbContext
{
    public CompartilheiDbContext(
        DbContextOptions<CompartilheiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Album> Albums => Set<Album>();

    public DbSet<Photo> Photos => Set<Photo>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CompartilheiDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}