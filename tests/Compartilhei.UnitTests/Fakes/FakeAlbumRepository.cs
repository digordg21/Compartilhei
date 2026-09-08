using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Domain.Entities;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakeAlbumRepository : IAlbumRepository
{
    private readonly List<Album> _albums = [];

    private readonly HashSet<Guid> _existingEvents = [];

    public bool AddCalled { get; private set; }

    public void SeedEvent(Guid eventId)
    {
        _existingEvents.Add(eventId);
    }

    public void SeedAlbums(int count)
    {
        AlbumCount = count;
    }

    public void SeedAlbum(Album album)
    {
        _albums.Add(album);
    }

    public Task<bool> EventExistsAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _existingEvents.Contains(eventId));
    }

    public Task AddAsync(
        Album album,
        CancellationToken cancellationToken)
    {
        AddCalled = true;

        _albums.Add(album);

        return Task.CompletedTask;
    }

    public int AlbumCount { get; private set; }

    public Task<int> CountByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(AlbumCount);
    }

    public Task<IReadOnlyList<Album>> GetActiveByEventIdAsync(
    Guid eventId,
    CancellationToken cancellationToken)
    {
        IReadOnlyList<Album> result = _albums
            .Where(x => x.EventId == eventId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Album?> GetActiveByIdAsync(
        Guid albumId,
        CancellationToken cancellationToken)
    {
        var album = _albums.FirstOrDefault(
            x => x.Id == albumId && x.IsActive);

        return Task.FromResult(album);
    }
}