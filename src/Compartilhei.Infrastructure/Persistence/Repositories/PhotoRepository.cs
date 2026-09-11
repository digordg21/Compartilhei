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
                 photo.Id < cursorId.Value));
        }

        return await query
            .OrderByDescending(photo => photo.CreatedAt)
            .ThenByDescending(photo => photo.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Photo>> GetAwaitingProcessingAsync(
    CancellationToken cancellationToken)
    {
        return await _dbContext.Photos
            .Where(photo =>
                photo.Status == PhotoStatus.Uploaded ||
                photo.Status == PhotoStatus.Processing)
            .OrderBy(photo => photo.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Photo>> GetAvailableByIdsAsync(
        IReadOnlyCollection<Guid> photoIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Photos
            .AsNoTracking()
            .Where(photo =>
                photoIds.Contains(photo.Id) &&
                photo.Status == PhotoStatus.Available)
            .ToListAsync(cancellationToken);
    }
}