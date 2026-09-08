namespace Compartilhei.Application.Photos.Processing;

public sealed record PhotoProcessingRequest(
    Guid PhotoId,
    string OriginalPath,
    string DisplayPath,
    string ThumbnailPath);