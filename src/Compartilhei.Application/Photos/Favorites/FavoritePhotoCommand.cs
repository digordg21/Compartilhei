namespace Compartilhei.Application.Photos.Favorites;

public sealed record FavoritePhotoCommand(
    string EventSlug,
    Guid AlbumId,
    Guid PhotoId);