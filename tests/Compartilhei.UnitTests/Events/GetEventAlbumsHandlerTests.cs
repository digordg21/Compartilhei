using Compartilhei.Application.Events.GetEventAlbums;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Entities;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Events;

public class GetEventAlbumsHandlerTests
{
    [Fact]
    public async Task Should_Return_Active_Albums_In_Order()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(
            new Event(
                "Casamento Rodrigo & Jennifer",
                "casamento-rodrigo-jennifer"));

        var albumRepository = new FakeAlbumRepository();

        albumRepository.SeedEvent(eventId);

        // TODO: fake precisa permitir albums
        // para este teste, vamos ajustar o fake no próximo passo.

        var handler = new GetEventAlbumsHandler(
            albumRepository,
            eventRepository);

        // Act
        // Assert
    }

    [Fact]
    public async Task Should_Return_Active_Albums_In_Display_Order()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var eventId = eventEntity.Id;

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedEvent(eventId);

        var firstAlbum = new Album(
            eventId,
            "Recepção",
            2);

        var secondAlbum = new Album(
            eventId,
            "Cerimônia",
            1);

        var inactiveAlbum = new Album(
            eventId,
            "Making Of",
            3);

        inactiveAlbum.Deactivate();

        albumRepository.SeedAlbum(firstAlbum);
        albumRepository.SeedAlbum(secondAlbum);
        albumRepository.SeedAlbum(inactiveAlbum);

        var handler = new GetEventAlbumsHandler(
            albumRepository,
            eventRepository);

        // Act
        var result = await handler.HandleAsync(
            new GetEventAlbumsQuery(eventId),
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("Cerimônia", result[0].Name);
        Assert.Equal(1, result[0].DisplayOrder);

        Assert.Equal("Recepção", result[1].Name);
        Assert.Equal(2, result[1].DisplayOrder);

        Assert.All(
            result,
            album => Assert.True(album.IsActive));
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_Event_Has_No_Active_Albums()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var eventId = eventEntity.Id;

        var eventRepository = new FakeEventRepository();
        eventRepository.Seed(eventEntity);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedEvent(eventId);

        var album = new Album(
            eventId,
            "Cerimônia",
            1);

        album.Deactivate();

        albumRepository.SeedAlbum(album);

        var handler = new GetEventAlbumsHandler(
            albumRepository,
            eventRepository);

        // Act
        var result = await handler.HandleAsync(
            new GetEventAlbumsQuery(eventId),
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}