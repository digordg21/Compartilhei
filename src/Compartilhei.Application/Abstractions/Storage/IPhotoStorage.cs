namespace Compartilhei.Application.Abstractions.Storage;

public interface IPhotoStorage
{
    Task<PhotoUploadAuthorization> CreateUploadAuthorizationAsync(
        Guid eventId,
        Guid albumId,
        Guid photoId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        string blobPath,
        CancellationToken cancellationToken);

    Task<string> CreateReadUrlAsync(
        string blobPath,
        CancellationToken cancellationToken);
}