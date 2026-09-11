using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Compartilhei.Infrastructure.Persistence.Repositories;

public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly CompartilheiDbContext _dbContext;

    public FavoriteRepository(CompartilheiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Favorite favorite,
        CancellationToken cancellationToken)
    {
        await _dbContext.Favorites.AddAsync(
            favorite,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public Task<Favorite?> GetByPhotoAndGuestSessionAsync(
        Guid photoId,
        Guid guestSessionId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Favorites.FirstOrDefaultAsync(
            favorite =>
                favorite.PhotoId == photoId &&
                favorite.GuestSessionId == guestSessionId,
            cancellationToken);
    }

    public async Task RemoveAsync(
        Favorite favorite,
        CancellationToken cancellationToken)
    {
        _dbContext.Favorites.Remove(favorite);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, PhotoFavoriteSummary>>
        GetSummariesByPhotoIdsAsync(
            IReadOnlyCollection<Guid> photoIds,
            Guid guestSessionId,
            CancellationToken cancellationToken)
    {
        var ids = photoIds.Distinct().ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<Guid, PhotoFavoriteSummary>();
        }

        var rows = await _dbContext.Favorites
            .AsNoTracking()
            .Where(favorite => ids.Contains(favorite.PhotoId))
            .GroupBy(favorite => favorite.PhotoId)
            .Select(group => new
            {
                PhotoId = group.Key,
                Count = group.Count(),
                IsFavorited = group.Any(
                    favorite =>
                        favorite.GuestSessionId == guestSessionId)
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            row => row.PhotoId,
            row => new PhotoFavoriteSummary(
                row.Count,
                row.IsFavorited));
    }

    public async Task<IReadOnlyList<Favorite>>
    GetByAlbumAndGuestSessionAsync(
        Guid albumId,
        Guid guestSessionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Favorites
            .AsNoTracking()
            .Include(favorite => favorite.Photo)
            .Where(favorite =>
                favorite.GuestSessionId == guestSessionId &&
                favorite.Photo.AlbumId == albumId &&
                favorite.Photo.Status == PhotoStatus.Available)
            .OrderByDescending(favorite => favorite.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>>
    GetPhotoIdsByAlbumAndGuestSessionAsync(
        Guid albumId,
        Guid guestSessionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Favorites
            .AsNoTracking()
            .Where(favorite =>
                favorite.GuestSessionId == guestSessionId &&
                favorite.Photo.AlbumId == albumId &&
                favorite.Photo.Status == PhotoStatus.Available)
            .OrderByDescending(favorite => favorite.CreatedAt)
            .Select(favorite => favorite.PhotoId)
            .ToListAsync(cancellationToken);
    }
}