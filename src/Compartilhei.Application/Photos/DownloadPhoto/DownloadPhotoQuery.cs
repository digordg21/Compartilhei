namespace Compartilhei.Application.Photos.DownloadPhoto;

public sealed record DownloadPhotoQuery(
    string EventSlug,
    Guid AlbumId,
    Guid PhotoId);