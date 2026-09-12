namespace Compartilhei.Application.Photos.Favorites.DownloadFavorites;

public sealed record DownloadFavoritesResult(
    Stream Content,
    string FileName,
    string ContentType);