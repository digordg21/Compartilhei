using Compartilhei.Domain.Entities;

namespace Compartilhei.Application.Abstractions.Persistence;

public interface IAlbumRepository
{
    Task AddAsync(
        Album album,
        CancellationToken cancellationToken);

    Task<bool> EventExistsAsync(
        Guid eventId,
        CancellationToken cancellationToken);

    Task<int> CountByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Album>> GetActiveByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken);

    Task<Album?> GetActiveByIdAsync(
        Guid albumId, 
        CancellationToken cancellationToken);
}