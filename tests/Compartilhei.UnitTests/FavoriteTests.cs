using Compartilhei.Domain.Entities;

namespace Compartilhei.UnitTests;

public class FavoriteTests
{
    [Fact]
    public void Should_Create_Favorite_When_Data_Is_Valid()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        var guestSessionId = Guid.NewGuid();

        // Act
        var favorite = new Favorite(
            photoId,
            guestSessionId);

        // Assert
        Assert.NotEqual(Guid.Empty, favorite.Id);
        Assert.Equal(photoId, favorite.PhotoId);
        Assert.Equal(guestSessionId, favorite.GuestSessionId);
        Assert.True(favorite.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Should_Reject_Empty_PhotoId()
    {
        // Act
        var act = () => new Favorite(
            Guid.Empty,
            Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_GuestSessionId()
    {
        // Act
        var act = () => new Favorite(
            Guid.NewGuid(),
            Guid.Empty);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}