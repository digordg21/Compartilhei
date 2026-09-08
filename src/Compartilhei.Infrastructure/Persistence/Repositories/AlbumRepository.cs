using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Compartilhei.Infrastructure.Persistence.Repositories;

public sealed class AlbumRepository : IAlbumRepository
{
    private readonly CompartilheiDbContext _dbContext;

    public AlbumRepository(
        CompartilheiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> EventExistsAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Events
            .AnyAsync(
                x => x.Id == eventId,
                cancellationToken);
    }

    public async Task<int> CountByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Albums
            .CountAsync(
                x => x.EventId == eventId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Album>> GetActiveByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .Where(x => x.EventId == eventId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Album?> GetActiveByIdAsync(
        Guid albumId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == albumId && x.IsActive,
                cancellationToken);
    }

    public async Task AddAsync(
        Album album,
        CancellationToken cancellationToken)
    {
        await _dbContext.Albums.AddAsync(
            album,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}