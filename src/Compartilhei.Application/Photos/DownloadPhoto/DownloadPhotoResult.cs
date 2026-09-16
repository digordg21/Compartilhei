namespace Compartilhei.Application.Photos.DownloadPhoto;

public sealed record DownloadPhotoResult(
    Stream Content,
    string FileName,
    string ContentType);