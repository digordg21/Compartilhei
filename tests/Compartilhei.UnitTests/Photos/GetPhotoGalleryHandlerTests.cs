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

        for (var index = 0; index < 31; index++)
        {
            await photoRepository.AddAsync(
                CreateAvailablePhoto(album.Id),
                CancellationToken.None);
        }

        var photoStorage = new FakePhotoStorage();

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage);

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

        var handler = new GetPhotoGalleryHandler(
            eventRepository,
            albumRepository,
            new FakePhotoRepository(),
            new FakePhotoStorage());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetPhotoGalleryQuery(
                    eventEntity.Slug,
                    album.Id,
                    null,
                    null),
                CancellationToken.None));
    }

    private static Photo CreateAvailablePhoto(Guid albumId)
    {
        var photo = new Photo(
            albumId,
            "foto.jpg",
            1024,
            Guid.NewGuid());

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