using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;

namespace Compartilhei.UnitTests;

public class PhotoTests
{
    [Fact]
    public void Should_Create_Photo_When_Data_Is_Valid()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var guestSessionId = Guid.NewGuid();

        // Act
        var photo = new Photo(
            albumId,
            "foto-casamento.jpg",
            2_000_000,
            guestSessionId);

        // Assert
        Assert.NotEqual(Guid.Empty, photo.Id);
        Assert.Equal(albumId, photo.AlbumId);
        Assert.Equal("foto-casamento.jpg", photo.FileName);
        Assert.Equal(2_000_000, photo.FileSize);
        Assert.Equal(guestSessionId, photo.UploadedBySessionId);
        Assert.Equal(PhotoStatus.Pending, photo.Status);

        Assert.Null(photo.Width);
        Assert.Null(photo.Height);
        Assert.Null(photo.OriginalPath);
        Assert.Null(photo.DisplayPath);
        Assert.Null(photo.ThumbnailPath);
        Assert.Null(photo.ProcessedAt);

        Assert.True(photo.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Should_Reject_Empty_AlbumId()
    {
        var act = () => new Photo(
            Guid.Empty,
            "foto.jpg",
            1000,
            Guid.NewGuid());

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_FileName()
    {
        var act = () => new Photo(
            Guid.NewGuid(),
            string.Empty,
            1000,
            Guid.NewGuid());

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Invalid_FileSize()
    {
        var act = () => new Photo(
            Guid.NewGuid(),
            "foto.jpg",
            0,
            Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_GuestSessionId()
    {
        var act = () => new Photo(
            Guid.NewGuid(),
            "foto.jpg",
            1000,
            Guid.Empty);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Invalid_Width()
    {
        var photo = CreatePhoto();

        var act = () => photo.SetDimensions(0, 1080);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Should_Reject_Invalid_Height()
    {
        var photo = CreatePhoto();

        var act = () => photo.SetDimensions(1920, 0);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Should_Set_Photo_Dimensions()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        photo.SetDimensions(1920, 1080);

        // Assert
        Assert.Equal(1920, photo.Width);
        Assert.Equal(1080, photo.Height);
    }

    [Fact]
    public void Should_Mark_Photo_As_Uploaded()
    {
        // Arrange
        var photo = CreatePhoto();

        const string originalPath =
            "events/event-1/albums/album-1/original/photo.jpg";

        // Act
        photo.MarkUploaded(originalPath);

        // Assert
        Assert.Equal(PhotoStatus.Uploaded, photo.Status);
        Assert.Equal(originalPath, photo.OriginalPath);
    }

    [Fact]
    public void Should_Reject_Empty_OriginalPath()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        var act = () => photo.MarkUploaded(string.Empty);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Mark_Photo_As_Processing()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        // Act
        photo.MarkProcessing();

        // Assert
        Assert.Equal(PhotoStatus.Processing, photo.Status);
    }

    [Fact]
    public void Should_Mark_Photo_As_Available()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        // Act
        photo.MarkAvailable(
            "events/event-1/albums/album-1/display/photo.jpg",
            "events/event-1/albums/album-1/thumbnail/photo.jpg");

        // Assert
        Assert.Equal(PhotoStatus.Available, photo.Status);

        Assert.Equal(
            "events/event-1/albums/album-1/display/photo.jpg",
            photo.DisplayPath);

        Assert.Equal(
            "events/event-1/albums/album-1/thumbnail/photo.jpg",
            photo.ThumbnailPath);

        Assert.Equal(1920, photo.Width);
        Assert.Equal(1080, photo.Height);

        Assert.NotNull(photo.ProcessedAt);
    }

    [Fact]
    public void Should_Reject_Empty_DisplayPath()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        // Act
        var act = () => photo.MarkAvailable(
            string.Empty,
            "thumbnail/photo.jpg");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_ThumbnailPath()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        // Act
        var act = () => photo.MarkAvailable(
            "display/photo.jpg",
            string.Empty);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Not_Mark_Photo_As_Available_Without_Dimensions()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        photo.MarkProcessing();

        // Act
        var act = () => photo.MarkAvailable(
            "display/photo.jpg",
            "thumbnail/photo.jpg");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Should_Mark_Photo_As_Failed()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        photo.MarkProcessing();

        // Act
        photo.MarkFailed();

        // Assert
        Assert.Equal(PhotoStatus.Failed, photo.Status);
    }

    [Fact]
    public void Should_Not_Allow_Pending_To_Processing()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        var act = () => photo.MarkProcessing();

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Contains(
            "Expected 'Uploaded'",
            exception.Message);
    }

    [Fact]
    public void Should_Not_Allow_Pending_To_Available()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        var act = () => photo.MarkAvailable(
            "display/photo.jpg",
            "thumbnail/photo.jpg");

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(act);

        Assert.Contains(
            "Expected 'Processing'",
            exception.Message);
    }

    [Fact]
    public void Should_Not_Allow_Uploaded_To_Available()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        // Act
        var act = () => photo.MarkAvailable(
            "display/photo.jpg",
            "thumbnail/photo.jpg");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Should_Not_Allow_Available_To_Failed()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        photo.MarkProcessing();
        photo.SetDimensions(1920, 1080);

        photo.MarkAvailable(
            "display/photo.jpg",
            "thumbnail/photo.jpg");

        // Act
        var act = () => photo.MarkFailed();

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Should_Not_Allow_Failed_To_Processing()
    {
        // Arrange
        var photo = CreatePhoto();

        photo.MarkUploaded(
            "events/event-1/albums/album-1/original/photo.jpg");

        photo.MarkProcessing();
        photo.MarkFailed();

        // Act
        var act = () => photo.MarkProcessing();

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    private static Photo CreatePhoto()
    {
        return new Photo(
            Guid.NewGuid(),
            "foto.jpg",
            2_000_000,
            Guid.NewGuid());
    }
}