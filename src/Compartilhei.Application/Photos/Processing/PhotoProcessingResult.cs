        namespace Compartilhei.Application.Photos.Processing;

public sealed record PhotoProcessingResult(
    string DisplayPath,
    string ThumbnailPath,
    int Width,
    int Height);