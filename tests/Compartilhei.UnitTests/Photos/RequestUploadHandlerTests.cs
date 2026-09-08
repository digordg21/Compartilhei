using Compartilhei.Application.Exceptions;
using Compartilhei.Application.Photos.RequestUpload;
using Compartilhei.Application.Photos.Validation;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Photos;

public class RequestUploadHandlerTests
{
    [Fact]
    public async Task Should_Create_Pending_Photo_And_Request_Upload_Authorization()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var albumRepository = new FakeAlbumRepository();
        albumRepository.SeedEvent(eventEntity.Id);
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();

        var photoStorage = new FakePhotoStorage();

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var validator = new PhotoUploadValidator();

        var handler = new RequestUploadHandler(
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            validator);

        var command = new RequestUploadCommand(
            eventEntity.Slug,
            album.Id,
            "foto-casamento.jpg",
            2_000_000,
            "image/jpeg");

        // Act
        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.PhotoId);

        Assert.False(
            string.IsNullOrWhiteSpace(result.BlobPath));

        Assert.False(
            string.IsNullOrWhiteSpace(result.UploadUrl));

        Assert.True(photoRepository.AddCalled);
        Assert.NotNull(photoRepository.AddedPhoto);

        Assert.Equal(
            PhotoStatus.Pending,
            photoRepository.AddedPhoto!.Status);

        Assert.Equal(
            guestSessionId,
            photoRepository.AddedPhoto.UploadedBySessionId);

        Assert.Equal(
            album.Id,
            photoRepository.AddedPhoto.AlbumId);

        Assert.True(photoStorage.Called);

        Assert.Equal(
            eventEntity.Id,
            photoStorage.EventId);

        Assert.Equal(
            album.Id,
            photoStorage.AlbumId);

        Assert.Equal(
            result.PhotoId,
            photoStorage.PhotoId);

        Assert.Equal(
            "foto-casamento.jpg",
            photoStorage.FileName);

        Assert.Equal(
            "image/jpeg",
            photoStorage.ContentType);
    }

    [Fact]
    public async Task Should_Reject_When_Album_Does_Not_Exist()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(Guid.NewGuid());

        var validator = new PhotoUploadValidator();

        var handler = new RequestUploadHandler(
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            validator);

        var command = new RequestUploadCommand(
            eventEntity.Slug,
            Guid.NewGuid(),
            "foto.jpg",
            1000,
            "image/jpeg");

        // Act
        var act = () => handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);

        Assert.False(photoRepository.AddCalled);
        Assert.False(photoStorage.Called);
    }

    [Fact]
    public async Task Should_Reject_When_Album_Is_Inactive()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        album.Deactivate();

        var albumRepository = new FakeAlbumRepository();

        albumRepository.SeedEvent(eventEntity.Id);
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(Guid.NewGuid());

        var validator = new PhotoUploadValidator();

        var handler = new RequestUploadHandler(
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            validator);

        var command = new RequestUploadCommand(
            eventEntity.Slug,
            album.Id,
            "foto.jpg",
            1000,
            "image/jpeg");

        // Act
        var act = () => handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);

        Assert.False(photoRepository.AddCalled);
        Assert.False(photoStorage.Called);
    }

    [Theory]
    [InlineData("foto.gif", "image/gif")]
    [InlineData("foto.bmp", "image/bmp")]
    [InlineData("foto.txt", "text/plain")]
    public async Task Should_Reject_Unsupported_File(
        string fileName,
        string contentType)
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var albumRepository = new FakeAlbumRepository();

        albumRepository.SeedEvent(eventEntity.Id);
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(Guid.NewGuid());

        var validator = new PhotoUploadValidator();

        var handler = new RequestUploadHandler(
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            validator);

        var command = new RequestUploadCommand(
            eventEntity.Slug,
            album.Id,
            fileName,
            1000,
            contentType);

        // Act
        var act = () => handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);

        Assert.False(photoRepository.AddCalled);
        Assert.False(photoStorage.Called);
    }

    [Fact]
    public async Task Should_Reject_File_Larger_Than_20MB()
    {
        // Arrange
        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var albumRepository = new FakeAlbumRepository();

        albumRepository.SeedEvent(eventEntity.Id);
        albumRepository.SeedAlbum(album);

        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(Guid.NewGuid());

        var validator = new PhotoUploadValidator();

        var handler = new RequestUploadHandler(
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            validator);

        var command = new RequestUploadCommand(
            eventEntity.Slug,
            album.Id,
            "foto.jpg",
            PhotoUploadValidator.MaxFileSizeBytes + 1,
            "image/jpeg");

        // Act
        var act = () => handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(act);

        Assert.False(photoRepository.AddCalled);
        Assert.False(photoStorage.Called);
    }
}