namespace Compartilhei.Application.Photos.Favorites.DownloadFavorites;

public sealed record DownloadFavoritesQuery(
    string EventSlug,
    Guid AlbumId);