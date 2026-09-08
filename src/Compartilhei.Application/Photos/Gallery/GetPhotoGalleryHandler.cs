using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Exceptions;

namespace Compartilhei.Application.Photos.Gallery;

public sealed class GetPhotoGalleryHandler
{
    private const int DefaultLimit = 30;
    private const int MaxLimit = 50;

    private readonly IEventRepository _eventRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoStorage _photoStorage;

    public GetPhotoGalleryHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        IPhotoStorage photoStorage)
    {
        _eventRepository = eventRepository;
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _photoStorage = photoStorage;
    }

    public async Task<PhotoGalleryResult> HandleAsync(
        GetPhotoGalleryQuery query,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetBySlugAsync(
            query.EventSlug,
            cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        var album = await _albumRepository.GetActiveByIdAsync(
            query.AlbumId,
            cancellationToken);

        if (album is null || album.EventId != eventEntity.Id)
        {
            throw new NotFoundException("Album not found.");
        }

        var limit = Math.Clamp(
            query.Limit ?? DefaultLimit,
            1,
            MaxLimit);

        var cursor = PhotoGalleryCursorCodec.Decode(query.Cursor);

        var photos = await _photoRepository.GetAvailableByAlbumAsync(
            album.Id,
            cursor?.CreatedAt,
            cursor?.Id,
            limit + 1,
            cancellationToken);

        var hasMore = photos.Count > limit;
        var page = photos.Take(limit).ToList();

        var items = new List<PhotoGalleryItemResult>(page.Count);

        foreach (var photo in page)
        {
            var thumbnailUrl = await _photoStorage.CreateReadUrlAsync(
                photo.ThumbnailPath!,
                cancellationToken);

            var displayUrl = await _photoStorage.CreateReadUrlAsync(
                photo.DisplayPath!,
                cancellationToken);

            items.Add(new PhotoGalleryItemResult(
                photo.Id,
                photo.FileName,
                thumbnailUrl,
                displayUrl,
                photo.Width!.Value,
                photo.Height!.Value,
                photo.CreatedAt));
        }

        string? nextCursor = null;

        if (hasMore)
        {
            var lastPhoto = page[^1];

            nextCursor = PhotoGalleryCursorCodec.Encode(
                new PhotoGalleryCursor(
                    lastPhoto.CreatedAt,
                    lastPhoto.Id));
        }

        return new PhotoGalleryResult(
            items,
            nextCursor,
            hasMore);
    }
}