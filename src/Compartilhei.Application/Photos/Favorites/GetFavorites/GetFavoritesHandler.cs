using Compartilhei.Application.Abstractions.Identity;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Exceptions;

namespace Compartilhei.Application.Photos.Favorites.GetFavorites;

public sealed class GetFavoritesHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoStorage _photoStorage;
    private readonly IGuestSessionAccessor _guestSessionAccessor;

    public GetFavoritesHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IFavoriteRepository favoriteRepository,
        IPhotoRepository photoRepository,
        IPhotoStorage photoStorage,
        IGuestSessionAccessor guestSessionAccessor)
    {
        _eventRepository = eventRepository;
        _albumRepository = albumRepository;
        _favoriteRepository = favoriteRepository;
        _photoRepository = photoRepository;
        _photoStorage = photoStorage;
        _guestSessionAccessor = guestSessionAccessor;
    }

    public async Task<GetFavoritesResult> HandleAsync(
        GetFavoritesQuery query,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetBySlugAsync(
            query.EventSlug,
            cancellationToken);

        if (eventEntity is null || !eventEntity.IsActive)
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

        var guestSessionId = _guestSessionAccessor.GuestSessionId;

        var photoIds =
            await _favoriteRepository
                .GetPhotoIdsByAlbumAndGuestSessionAsync(
                    album.Id,
                    guestSessionId,
                    cancellationToken);

        if (photoIds.Count == 0)
        {
            return new GetFavoritesResult([]);
        }

        var photos = await _photoRepository.GetAvailableByIdsAsync(
            photoIds,
            cancellationToken);

        var photoById = photos.ToDictionary(photo => photo.Id);

        var items = new List<GetFavoritesItemResult>(
            photoIds.Count);

        foreach (var photoId in photoIds)
        {
            if (!photoById.TryGetValue(photoId, out var photo))
            {
                continue;
            }

            var thumbnailUrl =
                await _photoStorage.CreateReadUrlAsync(
                    photo.ThumbnailPath!,
                    cancellationToken);

            var displayUrl =
                await _photoStorage.CreateReadUrlAsync(
                    photo.DisplayPath!,
                    cancellationToken);

            items.Add(
                new GetFavoritesItemResult(
                    photo.Id,
                    photo.FileName,
                    thumbnailUrl,
                    displayUrl,
                    photo.Width!.Value,
                    photo.Height!.Value,
                    photo.CreatedAt));
        }

        return new GetFavoritesResult(items);
    }
}