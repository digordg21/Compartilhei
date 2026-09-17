using Compartilhei.Application.Abstractions.Compression;
using Compartilhei.Application.Abstractions.Identity;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Exceptions;

namespace Compartilhei.Application.Photos.Favorites.DownloadFavorites;

public sealed class DownloadFavoritesHandler
{
    private const int MaxFavoritePhotos = 200;

    private readonly IEventRepository _eventRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IPhotoFileStorage _photoFileStorage;
    private readonly IZipArchiveService _zipArchiveService;
    private readonly IGuestSessionAccessor _guestSessionAccessor;

    public DownloadFavoritesHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IFavoriteRepository favoriteRepository,
        IPhotoRepository photoRepository,
        IPhotoFileStorage photoFileStorage,
        IZipArchiveService zipArchiveService,
        IGuestSessionAccessor guestSessionAccessor)
    {
        _eventRepository = eventRepository;
        _favoriteRepository = favoriteRepository;
        _photoRepository = photoRepository;
        _photoFileStorage = photoFileStorage;
        _zipArchiveService = zipArchiveService;
        _guestSessionAccessor = guestSessionAccessor;
    }

    public async Task<DownloadFavoritesResult> HandleAsync(
        DownloadFavoritesQuery query,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetBySlugAsync(
            query.EventSlug,
            cancellationToken);

        if (eventEntity is null || !eventEntity.IsActive)
        {
            throw new NotFoundException("Event not found.");
        }


        var guestSessionId = _guestSessionAccessor.GuestSessionId;

        var photoIds =
            await _favoriteRepository
                .GetPhotoIdsByEventAndGuestSessionAsync(
                    eventEntity.Id,
                    guestSessionId,
                    cancellationToken);

        if (photoIds.Count == 0)
        {
            throw new BusinessRuleException(
                "There are no favorite photos to download.");
        }

        if (photoIds.Count > MaxFavoritePhotos)
        {
            throw new BusinessRuleException(
                $"A maximum of {MaxFavoritePhotos} favorite photos can be downloaded at once.");
        }

        var photos = await _photoRepository.GetAvailableByIdsAsync(
            photoIds,
            cancellationToken);

        var photosById = photos.ToDictionary(
            photo => photo.Id);

        var entries = new List<ZipArchiveEntrySource>(
            photos.Count);

        var usedFileNames = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var photoId in photoIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!photosById.TryGetValue(
                    photoId,
                    out var photo))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(photo.OriginalPath))
            {
                continue;
            }

            var fileName = CreateUniqueFileName(
                photo.FileName,
                usedFileNames);

            var content = await _photoFileStorage.OpenReadAsync(
                photo.OriginalPath,
                cancellationToken);

            entries.Add(
                new ZipArchiveEntrySource(
                    fileName,
                    content));
        }

        if (entries.Count == 0)
        {
            throw new BusinessRuleException(
                "There are no available favorite photos to download.");
        }

        var zipStream = await _zipArchiveService.CreateAsync(
            entries,
            cancellationToken);

        foreach (var entry in entries)
        {
            await entry.Content.DisposeAsync();
        }

        return new DownloadFavoritesResult(
            zipStream,
            "favoritos.zip",
            "application/zip");
    }

    private static string CreateUniqueFileName(
        string fileName,
        HashSet<string> usedFileNames)
    {
        var safeFileName = Path.GetFileName(fileName);

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "foto";
        }

        if (usedFileNames.Add(safeFileName))
        {
            return safeFileName;
        }

        var extension = Path.GetExtension(safeFileName);
        var name = Path.GetFileNameWithoutExtension(safeFileName);

        var counter = 2;

        while (true)
        {
            var candidate =
                $"{name} ({counter}){extension}";

            if (usedFileNames.Add(candidate))
            {
                return candidate;
            }

            counter++;
        }
    }
}