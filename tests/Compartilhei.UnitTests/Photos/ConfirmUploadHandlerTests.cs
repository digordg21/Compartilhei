using Compartilhei.Application.Abstractions.Identity;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Exceptions;
using Compartilhei.Application.Photos.ConfirmUpload;
using Compartilhei.Domain.Entities;
using Compartilhei.Domain.Enums;
using Compartilhei.UnitTests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compartilhei.UnitTests.Photos;
public class ConfirmUploadHandlerTests
{
    [Fact]
    public async Task Should_confirm_upload_when_blob_exists()
    {
        // Arrange

        var processingQueue = new FakePhotoProcessingQueue();
        var eventRepository = new FakeEventRepository();
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        await eventRepository.AddAsync(
            eventEntity,
            CancellationToken.None);

        await albumRepository.AddAsync(
            album,
            CancellationToken.None);

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1024,
            guestSessionId);

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        photoStorage.BlobExists = true;

        var handler = new ConfirmUploadHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            processingQueue);

        var command = new ConfirmUploadCommand(
            eventEntity.Slug,
            album.Id,
            photo.Id);

        // Act

        var result = await handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert

        Assert.Equal(
            photo.Id,
            result.PhotoId);

        Assert.Equal(
            "Uploaded",
            result.Status);

        Assert.Equal(
            PhotoStatus.Uploaded,
            photo.Status);

        Assert.NotNull(
            photo.OriginalPath);

        Assert.True(
            photoRepository.UpdateCalled);

        Assert.Same(
            photo,
            photoRepository.UpdatedPhoto);

        Assert.Contains(
            photo.Id,
            processingQueue.EnqueuedPhotoIds);
    }

    [Fact]
    public async Task Should_reject_when_blob_does_not_exist()
    {
        // Arrange

        var processingQueue = new FakePhotoProcessingQueue();
        var eventRepository = new FakeEventRepository();
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1024,
            guestSessionId);

        await eventRepository.AddAsync(
            eventEntity,
            CancellationToken.None);

        await albumRepository.AddAsync(
            album,
            CancellationToken.None);

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var handler = new ConfirmUploadHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            processingQueue);

        var command = new ConfirmUploadCommand(
            eventEntity.Slug,
            album.Id,
            photo.Id);

        photoStorage.BlobExists = false;

        // Act + Assert

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));

        Assert.Equal(
            PhotoStatus.Pending,
            photo.Status);

        Assert.False(
            photoRepository.UpdateCalled);

        Assert.Empty(
            processingQueue.EnqueuedPhotoIds);
    }

    [Fact]
    public async Task Should_reject_when_event_does_not_exist()
    {
        // Arrange

        var processingQueue = new FakePhotoProcessingQueue();
        var eventRepository = new FakeEventRepository();
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var command = new ConfirmUploadCommand(
            "evento-inexistente",
            Guid.NewGuid(),
            Guid.NewGuid());

        var handler = new ConfirmUploadHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            processingQueue);

        // Act + Assert

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));
    }

    [Fact]
    public async Task Should_reject_when_album_does_not_exist()
    {
        // Arrange

        var processingQueue = new FakePhotoProcessingQueue();
        var eventRepository = new FakeEventRepository();
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        await eventRepository.AddAsync(eventEntity, CancellationToken.None);

        var command = new ConfirmUploadCommand(
            eventEntity.Slug,
            Guid.NewGuid(),
            Guid.NewGuid());

        var handler = new ConfirmUploadHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            processingQueue);

        // Act + Assert

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));
    }

    [Fact]
    public async Task Should_reject_when_photo_does_not_exist()
    {
        // Arrange

        var processingQueue = new FakePhotoProcessingQueue();
        var eventRepository = new FakeEventRepository();
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var command = new ConfirmUploadCommand(
            eventEntity.Slug,
            album.Id,
            Guid.NewGuid());

        var handler = new ConfirmUploadHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            processingQueue);

        // Act + Assert

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));
    }

    [Fact]
    public async Task Should_reject_when_photo_belongs_to_another_session()
    {
        // Arrange

        var processingQueue = new FakePhotoProcessingQueue();
        var eventRepository = new FakeEventRepository();
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var photoOwnerSessionId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        // criar Photo usando photoOwnerSessionId

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(currentSessionId);

        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1024,
            photoOwnerSessionId);

        await eventRepository.AddAsync(
            eventEntity,
            CancellationToken.None);

        await albumRepository.AddAsync(
            album,
            CancellationToken.None);

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        var command = new ConfirmUploadCommand(
            eventEntity.Slug,
            album.Id,
            photo.Id);

        var handler = new ConfirmUploadHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            processingQueue);

        // Act + Assert

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));

        Assert.Equal(
            PhotoStatus.Pending,
            photo.Status);
    }

    [Fact]
    public async Task Should_reject_when_photo_is_not_pending()
    {
        // Arrange

        var processingQueue = new FakePhotoProcessingQueue();
        var eventRepository = new FakeEventRepository();
        var albumRepository = new FakeAlbumRepository();
        var photoRepository = new FakePhotoRepository();
        var photoStorage = new FakePhotoStorage();

        var guestSessionId = Guid.NewGuid();

        var guestSessionAccessor =
            new FakeGuestSessionAccessor(guestSessionId);

        var eventEntity = new Event(
            "Casamento Rodrigo & Jennifer",
            "casamento-rodrigo-jennifer");

        var album = new Album(
            eventEntity.Id,
            "Cerimônia",
            1);

        var photo = new Photo(
            album.Id,
            "foto.jpg",
            1024,
            guestSessionId);

        await eventRepository.AddAsync(
            eventEntity,
            CancellationToken.None);

        await albumRepository.AddAsync(
            album,
            CancellationToken.None);

        await photoRepository.AddAsync(
            photo,
            CancellationToken.None);

        photo.MarkUploaded(
            $"events/{eventEntity.Id}/albums/{album.Id}/original/{photo.Id}.jpg");

        var handler = new ConfirmUploadHandler(
            eventRepository,
            albumRepository,
            photoRepository,
            photoStorage,
            guestSessionAccessor,
            processingQueue);

        var command = new ConfirmUploadCommand(
            eventEntity.Slug,
            album.Id,
            photo.Id);

        // Act + Assert

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => handler.HandleAsync(
                command,
                CancellationToken.None));

        Assert.Equal(
            "A foto não está aguardando confirmação de upload.",
            exception.Message);

        Assert.Equal(
            PhotoStatus.Uploaded,
            photo.Status);

        Assert.False(
            photoRepository.UpdateCalled);

}

}