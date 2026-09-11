namespace Compartilhei.Api.Contracts.Photos.Gallery;

public sealed record PhotoGalleryResponse(
    IReadOnlyList<PhotoGalleryItemResponse> Items,
    string? NextCursor,
    bool HasMore);

public sealed record PhotoGalleryItemResponse(
    Guid Id,
    string FileName,
    string ThumbnailUrl,
    string DisplayUrl,
    int Width,
    int Height,
    DateTimeOffset CreatedAt,
    bool IsFavorite);