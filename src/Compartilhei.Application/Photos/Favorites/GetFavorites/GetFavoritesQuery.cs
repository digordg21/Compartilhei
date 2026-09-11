namespace Compartilhei.Application.Photos.Favorites.GetFavorites;

public sealed record GetFavoritesQuery(
    string EventSlug,
    Guid AlbumId);