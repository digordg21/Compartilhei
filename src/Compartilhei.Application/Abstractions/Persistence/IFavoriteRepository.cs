using Compartilhei.Domain.Entities;

namespace Compartilhei.Application.Abstractions.Persistence;

public interface IFavoriteRepository
{
    Task AddAsync(
        Favorite favorite,
        CancellationToken cancellationToken);

    Task<Favorite?> GetByPhotoAndGuestSessionAsync(
        Guid photoId,
        Guid guestSessionId,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        Favorite favorite,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, PhotoFavoriteSummary>>
        GetSummariesByPhotoIdsAsync(
            IReadOnlyCollection<Guid> photoIds,
            Guid guestSessionId,
            CancellationToken cancellationToken);

    Task<IReadOnlyList<Favorite>> GetByAlbumAndGuestSessionAsync(
        Guid albumId,
        Guid guestSessionId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetPhotoIdsByAlbumAndGuestSessionAsync(
        Guid albumId,
        Guid guestSessionId,
        CancellationToken cancellationToken);
}



public sealed record PhotoFavoriteSummary(
    int Count,
    bool IsFavorited);