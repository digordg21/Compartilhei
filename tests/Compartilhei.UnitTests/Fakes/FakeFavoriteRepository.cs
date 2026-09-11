using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakeFavoriteRepository : IFavoriteRepository
{
    private readonly List<Favorite> _favorites = [];

    public Task AddAsync(
        Favorite favorite,
        CancellationToken cancellationToken)
    {
        _favorites.Add(favorite);

        return Task.CompletedTask;
    }

    public Task<Favorite?> GetByPhotoAndGuestSessionAsync(
        Guid photoId,
        Guid guestSessionId,
        CancellationToken cancellationToken)
    {
        var favorite = _favorites.FirstOrDefault(
            item =>
                item.PhotoId == photoId &&
                item.GuestSessionId == guestSessionId);

        return Task.FromResult(favorite);
    }

    public Task RemoveAsync(
        Favorite favorite,
        CancellationToken cancellationToken)
    {
        _favorites.Remove(favorite);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<Guid, PhotoFavoriteSummary>>
        GetSummariesByPhotoIdsAsync(
            IReadOnlyCollection<Guid> photoIds,
            Guid guestSessionId,
            CancellationToken cancellationToken)
    {
        var result = _favorites
            .Where(favorite => photoIds.Contains(favorite.PhotoId))
            .GroupBy(favorite => favorite.PhotoId)
            .ToDictionary(
                group => group.Key,
                group => new PhotoFavoriteSummary(
                    group.Count(),
                    group.Any(
                        favorite =>
                            favorite.GuestSessionId == guestSessionId)));

        return Task.FromResult<
            IReadOnlyDictionary<Guid, PhotoFavoriteSummary>>(result);
    }

    public Task<IReadOnlyList<Favorite>>
    GetByAlbumAndGuestSessionAsync(
        Guid albumId,
        Guid guestSessionId,
        CancellationToken cancellationToken)
    {
        var result = _favorites
            .Where(favorite =>
                favorite.GuestSessionId == guestSessionId &&
                favorite.Photo.AlbumId == albumId &&
                favorite.Photo.Status == PhotoStatus.Available)
            .OrderByDescending(favorite => favorite.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<Favorite>>(result);
    }

    public Task<IReadOnlyList<Guid>>
    GetPhotoIdsByAlbumAndGuestSessionAsync(
        Guid albumId,
        Guid guestSessionId,
        CancellationToken cancellationToken)
    {
        var photoIds = _favorites
            .Where(favorite =>
                favorite.GuestSessionId == guestSessionId)
            .Select(favorite => favorite.PhotoId)
            .ToList();

        return Task.FromResult<IReadOnlyList<Guid>>(photoIds);
    }
}