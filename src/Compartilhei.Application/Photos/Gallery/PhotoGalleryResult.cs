namespace Compartilhei.Application.Photos.Gallery;

public sealed record PhotoGalleryResult(
    IReadOnlyList<PhotoGalleryItemResult> Items,
    string? NextCursor,
    bool HasMore);

public sealed record PhotoGalleryItemResult(
    Guid Id,
    string FileName,
    string ThumbnailUrl,
    string DisplayUrl,
    int Width,
    int Height,
    DateTimeOffset CreatedAt);