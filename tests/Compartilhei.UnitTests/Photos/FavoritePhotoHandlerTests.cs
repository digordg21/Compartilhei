using Compartilhei.Application.Photos.Favorites;
using Compartilhei.Domain.Entities;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Photos;

public class FavoritePhotoHandlerTests
{
    [Fact]
    public async Task Should_add_favorite_when_photo_is_not_favorited()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var sessionId = Guid.NewGuid();

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1_000,
            sessionId);

        photo.MarkUploaded(
            $"events/event/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/event/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/event/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();
        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var favoriteRepository = new FakeFavoriteRepository();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(sessionId);

        var handler = new FavoritePhotoHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            favoriteRepository,
            guestSessionAccessor);

        var result = await handler.HandleAsync(
            new FavoritePhotoCommand(
                eventEntity.Slug,
                album.Id,
                photo.Id),
            CancellationToken.None);

        Assert.True(result.IsFavorited);
        Assert.Equal(1, result.FavoriteCount);
    }

    [Fact]
    public async Task Should_not_create_duplicate_favorite()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var sessionId = Guid.NewGuid();

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1_000,
            sessionId);

        photo.MarkUploaded(
            $"events/event/albums/{album.Id}/original/{photo.Id}.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            $"events/event/albums/{album.Id}/display/{photo.Id}.jpg",
            $"events/event/albums/{album.Id}/thumbnail/{photo.Id}.jpg");

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();
        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var favoriteRepository = new FakeFavoriteRepository();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(sessionId);

        var handler = new FavoritePhotoHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            favoriteRepository,
            guestSessionAccessor);

        await handler.HandleAsync(
            new FavoritePhotoCommand(
                eventEntity.Slug,
                album.Id,
                photo.Id),
            CancellationToken.None);

        var result = await handler.HandleAsync(
            new FavoritePhotoCommand(
                eventEntity.Slug,
                album.Id,
                photo.Id),
            CancellationToken.None);

        Assert.True(result.IsFavorited);
        Assert.Equal(1, result.FavoriteCount);
    }
}