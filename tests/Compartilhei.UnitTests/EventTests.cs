using Compartilhei.Domain.Entities;

namespace Compartilhei.UnitTests;

public class EventTests
{
    [Fact]
    public void Should_Create_Event_When_Data_Is_Valid()
    {
        // Arrange
        var name = "Casamento Rodrigo & Jennifer";
        var slug = "casamento-rodrigo-jennifer";

        // Act
        var eventEntity = new Event(name, slug);

        // Assert
        Assert.NotEqual(Guid.Empty, eventEntity.Id);
        Assert.Equal(name, eventEntity.Name);
        Assert.Equal(slug, eventEntity.Slug);
        Assert.True(eventEntity.IsActive);
        Assert.True(eventEntity.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Should_Reject_Empty_Name()
    {
        // Act
        var act = () => new Event(
            string.Empty,
            "casamento-rodrigo-Jennifer");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_Slug()
    {
        // Act
        var act = () => new Event(
            "Casamento Rodrigo & Jennifer",
            string.Empty);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Normalize_Slug()
    {
        // Act
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "  CASAMENTO-RODRIGO-Jennifer  ");

        // Assert
        Assert.Equal(
            "casamento-rodrigo-jennifer",
            eventEntity.Slug);
    }

    [Fact]
    public void Should_Deactivate_Event()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-Jennifer");

        // Act
        eventEntity.Deactivate();

        // Assert
        Assert.False(eventEntity.IsActive);
    }

    [Fact]
    public void Should_Activate_Event()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-Jennifer");

        eventEntity.Deactivate();

        // Act
        eventEntity.Activate();

        // Assert
        Assert.True(eventEntity.IsActive);
    }

    [Fact]
    public void Should_Rename_Event()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento",
            "casamento-rodrigo-Jennifer");

        // Act
        eventEntity.Rename("Casamento Rodrigo & Jennifer");

        // Assert
        Assert.Equal(
            "Casamento Rodrigo & Jennifer",
            eventEntity.Name);
    }
}