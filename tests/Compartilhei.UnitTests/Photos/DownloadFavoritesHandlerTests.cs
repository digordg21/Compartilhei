using Compartilhei.Application.Exceptions;
using Compartilhei.Application.Photos.Favorites.DownloadFavorites;
using Compartilhei.Domain.Entities;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Photos;

public class DownloadFavoritesHandlerTests
{
    [Fact]
    public async Task Should_reject_when_there_are_no_favorites()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var handler = new DownloadFavoritesHandler(
            eventRepository,
            albumRepository,
            new FakeFavoriteRepository(),
            new FakePhotoRepository(),
            new FakePhotoFileStorage(),
            new FakeZipArchiveService(),
            new FakeGuestSessionAccessor(Guid.NewGuid()));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.HandleAsync(
                new DownloadFavoritesQuery(
                    eventEntity.Slug,
                    album.Id),
                CancellationToken.None));

        Assert.Equal(
            "There are no favorite photos to download.",
            exception.Message);
    }

    [Fact]
    public async Task Should_create_zip_with_favorite_photos()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var guestSessionId = Guid.NewGuid();

        var photo = CreateAvailablePhoto(
            album.Id,
            guestSessionId);

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var favoriteRepository = new FakeFavoriteRepository();

        await favoriteRepository.AddAsync(
            new Favorite(
                photo.Id,
                guestSessionId),
            CancellationToken.None);

        var fileStorage = new FakePhotoFileStorage();

        fileStorage.Seed(
            photo.OriginalPath!,
            [1, 2, 3, 4]);

        var zipService = new FakeZipArchiveService();

        var handler = new DownloadFavoritesHandler(
            eventRepository,
            albumRepository,
            favoriteRepository,
            photoRepository,
            fileStorage,
            zipService,
            new FakeGuestSessionAccessor(guestSessionId));

        var result = await handler.HandleAsync(
            new DownloadFavoritesQuery(
                eventEntity.Slug,
                album.Id),
            CancellationToken.None);

        Assert.Equal(
            "favoritos.zip",
            result.FileName);

        Assert.Equal(
            "application/zip",
            result.ContentType);

        Assert.NotNull(result.Content);

        Assert.Single(zipService.Entries);
        Assert.Equal(
            photo.FileName,
            zipService.Entries.First().FileName);

        Assert.Single(fileStorage.OpenedBlobPaths);
        Assert.Equal(
            photo.OriginalPath,
            fileStorage.OpenedBlobPaths[0]);
    }

    [Fact]
    public async Task Should_reject_when_favorites_exceed_maximum_limit()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var guestSessionId = Guid.NewGuid();

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var favoriteRepository = new FakeFavoriteRepository();

        for (var index = 0; index < 201; index++)
        {
            var photoId = Guid.NewGuid();

            await favoriteRepository.AddAsync(
                new Favorite(
                    photoId,
                    guestSessionId),
                CancellationToken.None);
        }

        var handler = new DownloadFavoritesHandler(
            eventRepository,
            albumRepository,
            favoriteRepository,
            new FakePhotoRepository(),
            new FakePhotoFileStorage(),
            new FakeZipArchiveService(),
            new FakeGuestSessionAccessor(guestSessionId));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.HandleAsync(
                new DownloadFavoritesQuery(
                    eventEntity.Slug,
                    album.Id),
                CancellationToken.None));

        Assert.Equal(
            "A maximum of 200 favorite photos can be downloaded at once.",
            exception.Message);
    }

    [Fact]
    public async Task Should_generate_unique_file_names_for_duplicate_photo_names()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var guestSessionId = Guid.NewGuid();

        var photos = new List<Photo>();

        for (var index = 0; index < 3; index++)
        {
            var photo = CreateAvailablePhoto(
                album.Id,
                guestSessionId);

            photos.Add(photo);
        }

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();

        foreach (var photo in photos)
        {
            await photoRepository.AddAsync(
                photo,
                CancellationToken.None);
        }

        var favoriteRepository = new FakeFavoriteRepository();

        foreach (var photo in photos)
        {
            await favoriteRepository.AddAsync(
                new Favorite(
                    photo.Id,
                    guestSessionId),
                CancellationToken.None);
        }

        var fileStorage = new FakePhotoFileStorage();

        foreach (var photo in photos)
        {
            fileStorage.Seed(
                photo.OriginalPath!,
                [1, 2, 3]);
        }

        var zipService = new FakeZipArchiveService();

        var handler = new DownloadFavoritesHandler(
            eventRepository,
            albumRepository,
            favoriteRepository,
            photoRepository,
            fileStorage,
            zipService,
            new FakeGuestSessionAccessor(guestSessionId));

        var result = await handler.HandleAsync(
            new DownloadFavoritesQuery(
                eventEntity.Slug,
                album.Id),
            CancellationToken.None);

        var fileNames = zipService.Entries
            .Select(entry => entry.FileName)
            .ToList();

        Assert.Equal(3, fileNames.Count);
        Assert.Equal(
            3,
            fileNames.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.Equal(
            "foto.jpg",
            fileNames[0]);

        Assert.Equal(
            "foto (2).jpg",
            fileNames[1]);

        Assert.Equal(
            "foto (3).jpg",
            fileNames[2]);
    }

    private static Photo CreateAvailablePhoto(
        Guid albumId,
        Guid guestSessionId)
    {
        var photo = new Photo(
            albumId,
            "foto.jpg",
            1_000,
            guestSessionId);

        photo.MarkUploaded(
            $"events/event/albums/{albumId}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/event/albums/{albumId}/display/{photo.Id}.jpg",
            $"events/event/albums/{albumId}/thumbnail/{photo.Id}.jpg");

        return photo;
    }
}