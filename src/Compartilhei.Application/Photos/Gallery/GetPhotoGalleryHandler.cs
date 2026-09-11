using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Abstractions.Identity;
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
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IGuestSessionAccessor _guestSessionAccessor;

    public GetPhotoGalleryHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        IPhotoStorage photoStorage,
        IFavoriteRepository favoriteRepository,
        IGuestSessionAccessor guestSessionAccessor)
    {
        _eventRepository = eventRepository;
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _photoStorage = photoStorage;
        _favoriteRepository = favoriteRepository;
        _guestSessionAccessor = guestSessionAccessor;
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

        PhotoGalleryCursor? cursor = null;

        if (!string.IsNullOrWhiteSpace(query.Cursor) &&
            !PhotoGalleryCursorCodec.TryDecode(
                query.Cursor,
                out cursor))
        {
            throw new BusinessRuleException(
                "The gallery cursor is invalid.");
        }

        var photos = await _photoRepository.GetAvailableByAlbumAsync(
            album.Id,
            cursor?.CreatedAt,
            cursor?.Id,
            limit + 1,
            cancellationToken);

        var hasMore = photos.Count > limit;
        var page = photos.Take(limit).ToList();

        var guestSessionId = _guestSessionAccessor.GuestSessionId;

        var photoIds = page
            .Select(photo => photo.Id)
            .ToArray();

        var favoriteSummaries = await _favoriteRepository
            .GetSummariesByPhotoIdsAsync(
                photoIds,
                guestSessionId,
                cancellationToken);

        var items = new List<PhotoGalleryItemResult>(page.Count);

        foreach (var photo in page)
        {
            var thumbnailUrl = await _photoStorage.CreateReadUrlAsync(
                photo.ThumbnailPath!,
                cancellationToken);

            var displayUrl = await _photoStorage.CreateReadUrlAsync(
                photo.DisplayPath!,
                cancellationToken);

            favoriteSummaries.TryGetValue(
            photo.Id,
            out var favoriteSummary);

            items.Add(new PhotoGalleryItemResult(
                photo.Id,
                photo.FileName,
                thumbnailUrl,
                displayUrl,
                photo.Width!.Value,
                photo.Height!.Value,
                photo.CreatedAt,
                favoriteSummary?.IsFavorited ?? false));
        }

        string? nextCursor = null;

        if (hasMore && page.Count > 0)
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