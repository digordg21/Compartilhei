using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakePhotoRepository : IPhotoRepository
{
    private readonly List<Photo> _photos = [];

    public bool AddCalled { get; private set; }

    public Photo? AddedPhoto { get; private set; }

    public bool UpdateCalled { get; private set; }

    public Photo? UpdatedPhoto { get; private set; }

    public IReadOnlyCollection<Photo> Photos =>
        _photos.AsReadOnly();

    public Task AddAsync(
        Photo photo,
        CancellationToken cancellationToken)
    {
        AddCalled = true;
        AddedPhoto = photo;

        _photos.Add(photo);

        return Task.CompletedTask;
    }

    public Task<Photo?> GetByIdAsync(
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var photo = _photos.FirstOrDefault(
            item => item.Id == photoId);

        return Task.FromResult(photo);
    }

    public Task UpdateAsync(
        Photo photo,
        CancellationToken cancellationToken)
    {
        UpdateCalled = true;
        UpdatedPhoto = photo;

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Photo>> GetAvailableByAlbumAsync(
        Guid albumId,
        DateTimeOffset? cursorCreatedAt,
        Guid? cursorId,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _photos
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

        IReadOnlyList<Photo> result = query
            .OrderByDescending(photo => photo.CreatedAt)
            .ThenByDescending(photo => photo.Id)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Photo>> GetAwaitingProcessingAsync(
    CancellationToken cancellationToken)
    {
        IReadOnlyList<Photo> result = _photos
            .Where(photo =>
                photo.Status == PhotoStatus.Uploaded ||
                photo.Status == PhotoStatus.Processing)
            .OrderBy(photo => photo.CreatedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Photo>> GetAvailableByIdsAsync(
        IReadOnlyCollection<Guid> photoIds,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Photo> result = _photos
            .Where(photo => photoIds.Contains(photo.Id) &&
                            photo.Status == PhotoStatus.Available)
            .ToList();
        return Task.FromResult(result);
    }
}