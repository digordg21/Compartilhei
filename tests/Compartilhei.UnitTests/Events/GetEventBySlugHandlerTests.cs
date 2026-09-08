using Compartilhei.Application.Events.GetEventBySlug;
using Compartilhei.Application.Exceptions;
using Compartilhei.Domain.Entities;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Events;

public class GetEventBySlugHandlerTests
{
    [Fact]
    public async Task Should_Return_Event_When_Slug_Exists()
    {
        // Arrange
        var repository = new FakeEventRepository();

        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        repository.Seed(eventEntity);

        var handler = new GetEventBySlugHandler(repository);

        // Act
        var result = await handler.HandleAsync(
            new GetEventBySlugQuery(
                "casamento-rodrigo-jennifer"),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventEntity.Id, result.Id);
        Assert.Equal(eventEntity.Name, result.Name);
        Assert.Equal(eventEntity.Slug, result.Slug);
        Assert.True(result.IsActive);
        Assert.Null(result.CoverPhotoId);
    }

    [Fact]
    public async Task Should_Throw_NotFoundException_When_Slug_Does_Not_Exist()
    {
        // Arrange
        var repository = new FakeEventRepository();

        var handler = new GetEventBySlugHandler(repository);

        // Act
        var act = () => handler.HandleAsync(
            new GetEventBySlugQuery(
                "evento-inexistente"),
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}