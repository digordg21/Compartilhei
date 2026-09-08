using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;
using Compartilhei.Infrastructure.Persistence;
using Compartilhei.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Compartilhei.Infrastructure.Persistence.Repositories;

public sealed class PhotoRepository : IPhotoRepository
{
    private readonly CompartilheiDbContext _dbContext;

    public PhotoRepository(CompartilheiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Photo photo,
        CancellationToken cancellationToken)
    {
        await _dbContext.Photos.AddAsync(
            photo,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public Task<Photo?> GetByIdAsync(
        Guid photoId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Photos
            .FirstOrDefaultAsync(
                photo => photo.Id == photoId,
                cancellationToken);
    }

    public async Task UpdateAsync(
        Photo photo,
        CancellationToken cancellationToken)
    {
        _dbContext.Photos.Update(photo);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<Photo>> GetAvailableByAlbumAsync(
        Guid albumId,
        DateTimeOffset? cursorCreatedAt,
        Guid? cursorId,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Photos
            .AsNoTracking()
            .Where(photo =>
                photo.AlbumId == albumId &&
                photo.Status == PhotoStatus.Available);

        if (cursorCreatedAt.HasValue && cursorId.HasValue)
        {
            query = query.Where(photo =>
                photo.CreatedAt < cursorCreatedAt.Value ||
                (photo.CreatedAt == cursorCreatedAt.Value &&
                 photo.Id.CompareTo(cursorId.Value) < 0));
        }

        return await query
            .OrderByDescending(photo => photo.CreatedAt)
            .ThenByDescending(photo => photo.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}