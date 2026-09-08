using Compartilhei.Domain.Entities;

namespace Compartilhei.Application.Abstractions.Persistence;

public interface IPhotoRepository
{
    Task AddAsync(
        Photo photo,
        CancellationToken cancellationToken);

    Task<Photo?> GetByIdAsync(
        Guid photoId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Photo photo,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Photo>> GetAvailableByAlbumAsync(
        Guid albumId,
        DateTimeOffset? cursorCreatedAt,
        Guid? cursorId,
        int limit,
        CancellationToken cancellationToken);
}