namespace Compartilhei.Application.Photos.Gallery;

public sealed record GetPhotoGalleryQuery(
    string EventSlug,
    Guid AlbumId,
    string? Cursor,
    int? Limit);