using Compartilhei.Application.Photos.Favorites.GetFavorites;
using Compartilhei.Domain.Entities;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Photos;

public class GetFavoritesHandlerTests
{
    [Fact]
    public async Task Should_return_favorites_from_current_guest_session()
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

        var photoStorage = new FakePhotoStorage();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var handler = new GetFavoritesHandler(
            eventRepository,
            albumRepository,
            favoriteRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetFavoritesQuery(
                eventEntity.Slug,
                album.Id),
            CancellationToken.None);

        Assert.Single(result.Items);

        var item = result.Items[0];

        Assert.Equal(photo.Id, item.PhotoId);
        Assert.Equal(photo.FileName, item.FileName);
        Assert.Equal(photo.Width, item.Width);
        Assert.Equal(photo.Height, item.Height);

        Assert.NotNull(item.ThumbnailUrl);
        Assert.NotNull(item.DisplayUrl);

        Assert.Equal(2, photoStorage.ReadBlobPaths.Count);
    }

    [Fact]
    public async Task Should_return_empty_when_guest_has_no_favorites()
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

        var handler = new GetFavoritesHandler(
            eventRepository,
            albumRepository,
            new FakeFavoriteRepository(),
            new FakePhotoRepository(),
            new FakePhotoStorage(),
            new FakeGuestSessionAccessor(guestSessionId));

        var result = await handler.HandleAsync(
            new GetFavoritesQuery(
                eventEntity.Slug,
                album.Id),
            CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Should_not_return_favorites_from_another_guest_session()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var ownerSessionId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var photo = CreateAvailablePhoto(
            album.Id,
            ownerSessionId);

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
                ownerSessionId),
            CancellationToken.None);

        var handler = new GetFavoritesHandler(
            eventRepository,
            albumRepository,
            favoriteRepository,
            photoRepository,
            new FakePhotoStorage(),
            new FakeGuestSessionAccessor(currentSessionId));

        var result = await handler.HandleAsync(
            new GetFavoritesQuery(
                eventEntity.Slug,
                album.Id),
            CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Should_not_return_favorite_when_photo_is_not_available()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var guestSessionId = Guid.NewGuid();

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1_000,
            guestSessionId);

        photo.MarkUploaded(
            $"events/event/albums/{album.Id}/original/{photo.Id}.jpg");

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

        var handler = new GetFavoritesHandler(
            eventRepository,
            albumRepository,
            favoriteRepository,
            photoRepository,
            new FakePhotoStorage(),
            new FakeGuestSessionAccessor(guestSessionId));

        var result = await handler.HandleAsync(
            new GetFavoritesQuery(
                eventEntity.Slug,
                album.Id),
            CancellationToken.None);

        Assert.Empty(result.Items);
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