using Compartilhei.Application.Photos.Processing;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;
using Compartilhei.UnitTests.Fakes;

namespace Compartilhei.UnitTests.Photos;

public class RecoverPhotoProcessingHandlerTests
{
    [Fact]
    public async Task Should_requeue_uploaded_and_interrupted_processing_photos()
    {
        var repository = new FakePhotoRepository();
        var queue = new FakePhotoProcessingQueue();

        var uploadedPhoto = CreateUploadedPhoto();
        var processingPhoto = CreateUploadedPhoto();
        processingPhoto.MarkProcessing();

        var availablePhoto = CreateUploadedPhoto();
        availablePhoto.MarkProcessing();
        availablePhoto.SetDimensions(1920, 1080);
        availablePhoto.MarkAvailable(
            $"display/{availablePhoto.Id}.jpg",
            $"thumbnail/{availablePhoto.Id}.jpg");

        await repository.AddAsync(uploadedPhoto, CancellationToken.None);
        await repository.AddAsync(processingPhoto, CancellationToken.None);
        await repository.AddAsync(availablePhoto, CancellationToken.None);

        var handler = new RecoverPhotoProcessingHandler(
            repository,
            queue);

        var recoveredCount = await handler.HandleAsync(
            CancellationToken.None);

        Assert.Equal(2, recoveredCount);
        Assert.Equal(PhotoStatus.Uploaded, processingPhoto.Status);

        Assert.Contains(
            uploadedPhoto.Id,
            queue.EnqueuedPhotoIds);

        Assert.Contains(
            processingPhoto.Id,
            queue.EnqueuedPhotoIds);

        Assert.DoesNotContain(
            availablePhoto.Id,
            queue.EnqueuedPhotoIds);
    }

    private static Photo CreateUploadedPhoto()
    {
        var photo = new Photo(
            Guid.NewGuid(),
            "foto.jpg",
            1024,
            Guid.NewGuid());

        photo.MarkUploaded(
            $"events/event/albums/album/original/{photo.Id}.jpg");

        return photo;
    }
}