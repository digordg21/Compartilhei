using Compartilhei.Application.Events.CreateAlbum;
using Compartilhei.Application.Exceptions;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Events;

public class CreateAlbumHandlerTests
{
    [Fact]
    public async Task Should_Create_Album_When_Event_Exists()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var repository = new FakeAlbumRepository();

        repository.SeedEvent(eventId);

        var handler = new CreateAlbumHandler(repository);

        var command = new CreateAlbumCommand(
            eventId,
            "Cerimônia",
            1);

        // Act
        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);

        Assert.Equal(
            eventId,
            result.EventId);

        Assert.Equal(
            "Cerimônia",
            result.Name);

        Assert.Equal(
            1,
            result.DisplayOrder);

        Assert.True(result.IsActive);

        Assert.True(repository.AddCalled);
    }

    [Fact]
    public async Task Should_Reject_Album_When_Event_Does_Not_Exist()
    {
        // Arrange
        var repository = new FakeAlbumRepository();

        var handler = new CreateAlbumHandler(repository);

        var command = new CreateAlbumCommand(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        // Act
        var act = () => handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);

        Assert.False(repository.AddCalled);
    }

    [Fact]
    public async Task Should_Reject_Album_When_Event_Already_Has_4_Albums()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var repository = new FakeAlbumRepository();

        repository.SeedEvent(eventId);
        repository.SeedAlbums(4);

        var handler = new CreateAlbumHandler(repository);

        var command = new CreateAlbumCommand(
            eventId,
            "Cerimônia",
            1);

        // Act
        var act = () => handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<BusinessRuleException>(act);

        Assert.False(repository.AddCalled);
    }
}