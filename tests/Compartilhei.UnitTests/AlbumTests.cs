using Compartilhei.Domain.Entities;

namespace Compartilhei.UnitTests;

public class AlbumTests
{
    [Fact]
    public void Should_Create_Album_When_Data_Is_Valid()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var album = new Album(
            eventId,
            "Cerimônia",
            1);

        // Assert
        Assert.NotEqual(Guid.Empty, album.Id);
        Assert.Equal(eventId, album.EventId);
        Assert.Equal("Cerimônia", album.Name);
        Assert.Equal(1, album.DisplayOrder);
        Assert.True(album.IsActive);
        Assert.True(album.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Should_Reject_Empty_EventId()
    {
        // Act
        var act = () => new Album(
            Guid.Empty,
            "Cerimônia",
            1);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_Name()
    {
        // Act
        var act = () => new Album(
            Guid.NewGuid(),
            string.Empty,
            1);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Negative_DisplayOrder()
    {
        // Act
        var act = () => new Album(
            Guid.NewGuid(),
            "Cerimônia",
            -1);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Should_Rename_Album()
    {
        // Arrange
        var album = new Album(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        // Act
        album.Rename("Festa");

        // Assert
        Assert.Equal("Festa", album.Name);
    }

    [Fact]
    public void Should_Change_DisplayOrder()
    {
        // Arrange
        var album = new Album(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        // Act
        album.ChangeOrder(2);

        // Assert
        Assert.Equal(2, album.DisplayOrder);
    }

    [Fact]
    public void Should_Deactivate_Album()
    {
        // Arrange
        var album = new Album(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        // Act
        album.Deactivate();

        // Assert
        Assert.False(album.IsActive);
    }

    [Fact]
    public void Should_Activate_Album()
    {
        // Arrange
        var album = new Album(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        album.Deactivate();

        // Act
        album.Activate();

        // Assert
        Assert.True(album.IsActive);
    }
}