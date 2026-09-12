using Compartilhei.Application.Exceptions;
using Compartilhei.Application.Photos.Gallery;
using Compartilhei.Domain.Entities;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Photos;

public class GetPhotoGalleryHandlerTests
{
    [Fact]
    public async Task Should_return_a_page_and_next_cursor()
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

        var photoRepository = new FakePhotoRepository();

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        for (var index = 0; index < 31; index++)
        {
            await photoRepository.AddAsync(
                CreateAvailablePhoto(album.Id, 
                    FakeGuestSessionAccessor.GuestSessionId),
                    CancellationToken.None);
        }

        var photoStorage = new FakePhotoStorage();

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                30),
            CancellationToken.None);

        Assert.Equal(30, result.Items.Count);
        Assert.True(result.HasMore);
        Assert.NotNull(result.NextCursor);

        // Cada foto precisa de uma URL para miniatura e outra para display.
        Assert.Equal(60, photoStorage.ReadBlobPaths.Count);
    }

    [Fact]
    public async Task Should_return_second_page_using_next_cursor()
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

        var photoRepository = new FakePhotoRepository();

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        for (var index = 0; index < 31; index++)
        {
            await photoRepository.AddAsync(
                CreateAvailablePhoto(album.Id, 
                    FakeGuestSessionAccessor.GuestSessionId),
                    CancellationToken.None);
        }

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var firstPage = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                30),
            CancellationToken.None);

        Assert.True(firstPage.HasMore);
        Assert.NotNull(firstPage.NextCursor);

        var secondPage = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                firstPage.NextCursor,
                30),
            CancellationToken.None);

        Assert.Single(secondPage.Items);
        Assert.False(secondPage.HasMore);
        Assert.Null(secondPage.NextCursor);
    }

    [Fact]
    public async Task Should_return_last_page_without_next_cursor()
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

        var photoRepository = new FakePhotoRepository();

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        for (var index = 0; index < 30; index++)
        {
            await photoRepository.AddAsync(
                CreateAvailablePhoto(album.Id, 
                    FakeGuestSessionAccessor.GuestSessionId),
                    CancellationToken.None);
        }

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                30),
            CancellationToken.None);

        Assert.Equal(30, result.Items.Count);
        Assert.False(result.HasMore);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task Should_use_default_limit_when_limit_is_not_provided()
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

        var photoRepository = new FakePhotoRepository();

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        for (var index = 0; index < 31; index++)
        {
            await photoRepository.AddAsync(
                CreateAvailablePhoto(album.Id, 
                    FakeGuestSessionAccessor.GuestSessionId),
                    CancellationToken.None);
        }

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                null),
            CancellationToken.None);

        Assert.Equal(30, result.Items.Count);
        Assert.True(result.HasMore);
        Assert.NotNull(result.NextCursor);
    }

    [Fact]
    public async Task Should_limit_page_size_to_maximum_limit()
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

        var photoRepository = new FakePhotoRepository();

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        for (var index = 0; index < 60; index++)
        {
            await photoRepository.AddAsync(
                CreateAvailablePhoto(album.Id,
                    FakeGuestSessionAccessor.GuestSessionId),
                    CancellationToken.None);
        }

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                100),
            CancellationToken.None);

        Assert.Equal(50, result.Items.Count);
        Assert.True(result.HasMore);
        Assert.NotNull(result.NextCursor);
    }

    [Fact]
    public async Task Should_use_minimum_limit_when_limit_is_zero()
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

        var photoRepository = new FakePhotoRepository();

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        for (var index = 0; index < 5; index++)
        {
            await photoRepository.AddAsync(
                CreateAvailablePhoto(album.Id, 
                    FakeGuestSessionAccessor.GuestSessionId),
                    CancellationToken.None);
        }

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                0),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.True(result.HasMore);
        Assert.NotNull(result.NextCursor);
    }

    [Fact]
    public async Task Should_reject_invalid_cursor()
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

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            new FakePhotoRepository(),
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.HandleAsync(
                new GetPhotoGalleryQuery(
                    eventEntity.Slug,
                    album.Id,
                    "cursor-invalido",
                    null),
                CancellationToken.None));

        Assert.Equal(
            "The gallery cursor is invalid.",
            exception.Message);
    }

    [Fact]
    public async Task Should_reject_when_event_does_not_exist()
    {
        var albumId = Guid.NewGuid();

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        var handler = new GetPhotoGalleryHandler(
            new FakeEventRepository(),
            new FakeAlbumRepository(),
            new FakePhotoRepository(),
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetPhotoGalleryQuery(
                    "evento-inexistente",
                    albumId,
                    null,
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Should_reject_when_album_does_not_exist()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);
        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            new FakeAlbumRepository(),
            new FakePhotoRepository(),
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetPhotoGalleryQuery(
                    eventEntity.Slug,
                    Guid.NewGuid(),
                    null,
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Should_return_only_available_photos()
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

        var photoRepository = new FakePhotoRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        var availablePhoto = CreateAvailablePhoto(album.Id, FakeGuestSessionAccessor.GuestSessionId);

        var favoriteRepository = new FakeFavoriteRepository();

        var uploadedPhoto = new Photo(
            album.Id,
            "foto-uploaded.jpg",
            1024,
            Guid.NewGuid());

        uploadedPhoto.MarkUploaded(
            $"events/event/albums/{album.Id}/original/{uploadedPhoto.Id}.jpg");

        await photoRepository.AddAsync(
            availablePhoto,
            CancellationToken.None);

        await photoRepository.AddAsync(
            uploadedPhoto,
            CancellationToken.None);

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                null),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(availablePhoto.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task Should_return_photos_ordered_by_created_at_descending()
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

        var photoRepository = new FakePhotoRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        var olderPhoto = CreateAvailablePhoto(album.Id, FakeGuestSessionAccessor.GuestSessionId);
        var newerPhoto = CreateAvailablePhoto(album.Id, FakeGuestSessionAccessor.GuestSessionId);

        typeof(Photo)
            .GetProperty(nameof(Photo.CreatedAt))!
            .SetValue(
                olderPhoto,
                DateTimeOffset.UtcNow.AddMinutes(-10));

        typeof(Photo)
            .GetProperty(nameof(Photo.CreatedAt))!
            .SetValue(
                newerPhoto,
                DateTimeOffset.UtcNow);

        var favoriteRepository = new FakeFavoriteRepository();

        await photoRepository.AddAsync(
            olderPhoto,
            CancellationToken.None);

        await photoRepository.AddAsync(
            newerPhoto,
            CancellationToken.None);

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                null),
            CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(newerPhoto.Id, result.Items[0].Id);
        Assert.Equal(olderPhoto.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task Should_return_only_photos_from_requested_album()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var requestedAlbum = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var anotherAlbum = new Album(
            eventEntity.Id,
            "Festa",
            2);

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(requestedAlbum);
        albumRepository.SeedAlbum(anotherAlbum);

        var photoRepository = new FakePhotoRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        var requestedPhoto = CreateAvailablePhoto(requestedAlbum.Id, FakeGuestSessionAccessor.GuestSessionId);
        var anotherPhoto = CreateAvailablePhoto(anotherAlbum.Id, FakeGuestSessionAccessor.GuestSessionId);

        var favoriteRepository = new FakeFavoriteRepository();


        await photoRepository.AddAsync(
            requestedPhoto,
            CancellationToken.None);

        await photoRepository.AddAsync(
            anotherPhoto,
            CancellationToken.None);

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                requestedAlbum.Id,
                null,
                null),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(requestedPhoto.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task Should_reject_album_from_another_event()
    {
        var eventEntity = new Event(
            "Evento A",
            "evento-a");

        var anotherEvent = new Event(
            "Evento B",
            "evento-b");

        var album = new Album(
            anotherEvent.Id,
            "Álbum B",
            1);

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var favoriteRepository = new FakeFavoriteRepository();

        var FakeGuestSessionAccessor = new FakeGuestSessionAccessor(Guid.NewGuid());

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            new FakePhotoRepository(),
            new FakePhotoStorage(),
            favoriteRepository,
            FakeGuestSessionAccessor);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetPhotoGalleryQuery(
                    eventEntity.Slug,
                    album.Id,
                    null,
                    null),
                CancellationToken.None));
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

    [Fact]
    public async Task Should_mark_photo_as_favorite_for_current_guest_session()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor = new FakeGuestSessionAccessor(guestSessionId);

        var photo = CreateAvailablePhoto(album.Id, guestSessionId);

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

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            guestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                null),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.True(result.Items[0].IsFavorite);
    }

    [Fact]
    public async Task Should_not_mark_photo_as_favorite_for_another_guest_session()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var favoriteSessionId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var guestSessionAccessor = new FakeGuestSessionAccessor(currentSessionId);

        var photo = CreateAvailablePhoto(album.Id, guestSessionAccessor.GuestSessionId);

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
                favoriteSessionId),
            CancellationToken.None);

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            favoriteRepository,
            guestSessionAccessor);

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                null),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.False(result.Items[0].IsFavorite);
    }

    [Fact]
    public async Task Should_mark_photo_as_not_favorite_when_guest_has_not_favorited_it()
    {
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var guestSessionId = Guid.NewGuid();

        var photo = CreateAvailablePhoto(album.Id, guestSessionId);

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            new FakePhotoStorage(),
            new FakeFavoriteRepository(),
            new FakeGuestSessionAccessor(guestSessionId));

        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventEntity.Slug,
                album.Id,
                null,
                null),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.False(result.Items[0].IsFavorite);
    }
}