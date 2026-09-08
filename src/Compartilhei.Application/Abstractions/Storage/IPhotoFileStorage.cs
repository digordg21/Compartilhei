namespace Compartilhei.Application.Abstractions.Storage;

public interface IPhotoFileStorage
{
    Task<Stream> OpenReadAsync(
        string blobPath,
        CancellationToken cancellationToken);

    Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);
}