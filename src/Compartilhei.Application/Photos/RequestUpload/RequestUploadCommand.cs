namespace Compartilhei.Application.Photos.RequestUpload;

public sealed record RequestUploadCommand(
    string EventSlug,
    Guid AlbumId,
    string FileName,
    long FileSize,
    string ContentType);