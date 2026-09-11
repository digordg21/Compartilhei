using Compartilhei.Application.Abstractions.Identity;
using Compartilhei.Application.Abstractions.Persistence;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Exceptions;
using Compartilhei.Application.Photos.Validation;
using Compartilhei.Domain.Entities;

namespace Compartilhei.Application.Photos.RequestUpload;

public sealed class RequestUploadHandler
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IPhotoStorage _photoStorage;
    private readonly IGuestSessionAccessor _guestSessionAccessor;
    private readonly PhotoUploadValidator _validator;

    public RequestUploadHandler(
        IEventRepository eventRepository,
        IAlbumRepository albumRepository,
        IPhotoRepository photoRepository,
        IPhotoStorage photoStorage,
        IGuestSessionAccessor guestSessionAccessor,
        PhotoUploadValidator validator)
    {
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _photoStorage = photoStorage;
        _guestSessionAccessor = guestSessionAccessor;
        _validator = validator;
        _eventRepository = eventRepository;
    }

    public async Task<RequestUploadResult> HandleAsync(
        RequestUploadCommand command,
        CancellationToken cancellationToken)
    {
        _validator.Validate(
            command.FileName,
            command.FileSize,
            command.ContentType);

        var eventEntity = await _eventRepository.GetBySlugAsync(
            command.EventSlug,
            cancellationToken);

        if (eventEntity == null || !eventEntity.IsActive)
        {
            throw new NotFoundException(
                $"Event not found.");
        }

        var album = await _albumRepository.GetActiveByIdAsync(
            command.AlbumId,
            cancellationToken);

        if (album is null || album.EventId != eventEntity.Id)
        {
            throw new NotFoundException(
                $"Album not found.");
        }

        var guestSessionId =
            _guestSessionAccessor.GuestSessionId;

        var photo = new Photo(
            command.AlbumId,
            command.FileName,
            command.FileSize,
            guestSessionId);

        await _photoRepository.AddAsync(
            photo,
            cancellationToken);

        var authorization =
            await _photoStorage.CreateUploadAuthorizationAsync(
                eventEntity.Id,
                album.Id,
                photo.Id,
                command.FileName,
                command.ContentType,
                cancellationToken);

        return new RequestUploadResult(
            photo.Id,
            authorization.BlobPath,
            authorization.UploadUrl);
    }
}