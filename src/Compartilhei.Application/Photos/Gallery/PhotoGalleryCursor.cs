namespace Compartilhei.Application.Photos.Gallery;

public sealed record PhotoGalleryCursor(
    DateTimeOffset CreatedAt,
    Guid Id);