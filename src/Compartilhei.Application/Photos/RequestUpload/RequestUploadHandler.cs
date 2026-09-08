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
    private readonly IPhotoStorage _photoStorage;
    private readonly IGuestSessionAccessor _guestSessionAccessor;
    private readonly PhotoUploadValidator _validator;

    public RequestUploadHandler(
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
    }

    public async Task<RequestUploadResult> HandleAsync(
        RequestUploadCommand command,
        CancellationToken cancellationToken)
    {
        _validator.Validate(
            command.FileName,
            command.FileSize,
            command.ContentType);

        var album = await _albumRepository.GetActiveByIdAsync(
            command.AlbumId,
            cancellationToken);

        if (album is null)
        {
            throw new NotFoundException(
                $"Active album '{command.AlbumId}' was not found.");
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
                album.EventId,
                command.AlbumId,
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