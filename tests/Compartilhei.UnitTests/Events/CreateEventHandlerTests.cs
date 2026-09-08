using Compartilhei.Application.Events.CreateEvent;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Entities;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Events;

public class CreateEventHandlerTests
{
    [Fact]
    public async Task Should_Create_Event_When_Slug_Is_Available()
    {
        // Arrange
        var repository = new FakeEventRepository();

        var handler = new CreateEventHandler(repository);

        var command = new CreateEventCommand(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        // Act
        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(
            "Casamento Rodrigo & Jennifer",
            result.Name);

        Assert.Equal(
            "casamento-rodrigo-jennifer",
            result.Slug);

        Assert.True(repository.AddCalled);
    }

    [Fact]
    public async Task Should_Reject_Duplicate_Slug()
    {
        // Arrange
        var repository = new FakeEventRepository();

        repository.Seed(
            new Event(
                "Existing Event",
                "casamento-rodrigo-jennifer"));

        var handler = new CreateEventHandler(repository);

        var command = new CreateEventCommand(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        // Act
        var act = () => handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(act);

        Assert.False(repository.AddCalled);
    }
}