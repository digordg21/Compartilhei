namespace Compartilhei.Application.Photos.ConfirmUpload;

public sealed record ConfirmUploadCommand(
    string EventSlug,
    Guid AlbumId,
    Guid PhotoId);