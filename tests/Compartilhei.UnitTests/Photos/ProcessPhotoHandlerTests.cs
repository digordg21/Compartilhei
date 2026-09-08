using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Exceptions;
using Compartilhei.Application.Photos.Processing;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Photos;

public class ProcessPhotoHandlerTests
{
    [Fact]
    public async Task Should_process_uploaded_photo()
    {
        // Arrange

        var photoRepository = new FakePhotoRepository();



        var album = new Album(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1024,
            Guid.NewGuid());

        var processor = new FakePhotoProcessor
        {
            Result = new PhotoProcessingResult(
                $"events/event/albums/album/display/{photo.Id}.jpg",
                $"events/event/albums/album/thumbnail/{photo.Id}.jpg",
                1920,
                1080)
        };

        photo.MarkUploaded(
            $"events/event/albums/album/original/{photo.Id}.jpg");

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var handler = new ProcessPhotoHandler(
            photoRepository,
            processor);

        var command = new ProcessPhotoCommand(photo.Id);

        // Act

        await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert

        Assert.Equal(
            PhotoStatus.Available,
            photo.Status);

        Assert.Equal(
            1920,
            photo.Width);

        Assert.Equal(
            1080,
            photo.Height);

        Assert.Equal(
            $"events/event/albums/album/display/{photo.Id}.jpg",
            photo.DisplayPath);

        Assert.Equal(
            $"events/event/albums/album/thumbnail/{photo.Id}.jpg",
            photo.ThumbnailPath);

        Assert.True(
            photoRepository.UpdateCalled);

        Assert.Equal(
            1,
            processor.ProcessCallCount);
    }

    [Fact]
    public async Task Should_ignore_photo_when_not_uploaded()
    {
        // Arrange

        var photoRepository = new FakePhotoRepository();

        var processor = new FakePhotoProcessor();

        var album = new Album(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1024,
            Guid.NewGuid());

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var handler = new ProcessPhotoHandler(
            photoRepository,
            processor);

        var command = new ProcessPhotoCommand(photo.Id);

        // Act

        await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert

        Assert.Equal(
            PhotoStatus.Pending,
            photo.Status);

        Assert.Equal(
            0,
            processor.ProcessCallCount);
    }

    [Fact]
    public async Task Should_mark_photo_as_failed_when_processing_fails()
    {
        // Arrange

        var photoRepository = new FakePhotoRepository();

        var processor = new FakePhotoProcessor
        {
            Exception = new InvalidOperationException(
                "Image processing failed.")
        };

        var album = new Album(
            Guid.NewGuid(),
            "Cerimônia",
            1);

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1024,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/event/albums/album/original/{photo.Id}.jpg");

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var handler = new ProcessPhotoHandler(
            photoRepository,
            processor);

        var command = new ProcessPhotoCommand(photo.Id);

        // Act + Assert

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));

        Assert.Equal(
            PhotoStatus.Failed,
            photo.Status);

        Assert.True(
            photoRepository.UpdateCalled);
    }

    [Fact]
    public async Task Should_reject_when_photo_does_not_exist()
    {
        // Arrange

        var photoRepository = new FakePhotoRepository();

        var processor = new FakePhotoProcessor();

        var handler = new ProcessPhotoHandler(
            photoRepository,
            processor);

        var command = new ProcessPhotoCommand(
            Guid.NewGuid());

        // Act + Assert

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));

        Assert.Equal(
            0,
            processor.ProcessCallCount);
    }
}