using Compartilhei.Api.Contracts.Photos.RequestUpload;
using Compartilhei.Api.Contracts.Photos.ConfirmUpload;
using Compartilhei.Api.Contracts.Photos.Gallery;
using Compartilhei.Api.Contracts.Photos.Favorites;
using Compartilhei.Api.Contracts.Photos.Favorites.GetFavorites;
using Compartilhei.Application.Photos.Favorites.GetFavorites;
using Compartilhei.Application.Photos.Favorites;
using Compartilhei.Application.Photos.Favorites.DownloadFavorites;
using Compartilhei.Application.Photos.Gallery;
using Compartilhei.Application.Photos.RequestUpload;
using Compartilhei.Application.Photos.ConfirmUpload;
using Compartilhei.Application.Photos.DownloadPhoto;


using Microsoft.AspNetCore.Mvc;

namespace Compartilhei.Api.Controllers;

[ApiController]
[Route("api/events/{eventSlug}/albums/{albumId:guid}/photos")]
public sealed class PhotosController : ControllerBase
{
    [HttpPost("upload-request")]
    public async Task<ActionResult<RequestUploadResponse>> RequestUpload(
        string eventSlug,
        Guid albumId,
        RequestUploadRequest request,
        [FromServices] RequestUploadHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RequestUploadCommand(
            eventSlug,
            albumId,
            request.FileName,
            request.FileSize,
            request.ContentType);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return Ok(
            new RequestUploadResponse(
                result.PhotoId,
                result.BlobPath,
                result.UploadUrl));
    }

    [HttpPost("{photoId:guid}/confirm-upload")]
    public async Task<ActionResult<ConfirmUploadResponse>> ConfirmUpload(
        string eventSlug,
        Guid albumId,
        Guid photoId,
        [FromServices] ConfirmUploadHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmUploadCommand(
            eventSlug,
            albumId,
            photoId);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return Ok(
            new ConfirmUploadResponse(
                result.PhotoId,
                result.Status));
    }

    [HttpGet]
    public async Task<ActionResult<PhotoGalleryResponse>> GetGallery(
        string eventSlug,
        Guid albumId,
        [FromQuery] string? cursor,
        [FromQuery] int? limit,
        [FromServices] GetPhotoGalleryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetPhotoGalleryQuery(
                eventSlug,
                albumId,
                cursor,
                limit),
            cancellationToken);

        var items = result.Items
            .Select(item => new PhotoGalleryItemResponse(
                item.Id,
                item.FileName,
                item.ThumbnailUrl,
                item.DisplayUrl,
                item.Width,
                item.Height,
                item.CreatedAt,
                item.IsFavorite))
            .ToList();

        return Ok(new PhotoGalleryResponse(
            items,
            result.NextCursor,
            result.HasMore));
    }

    [HttpPost("{photoId:guid}/favorite")]
    public async Task<ActionResult<FavoritePhotoResponse>> Favorite(
        string eventSlug,
        Guid albumId,
        Guid photoId,
        [FromServices] FavoritePhotoHandler handler,
        CancellationToken cancellationToken)
     {
        var result = await handler.HandleAsync(
            new FavoritePhotoCommand(
                eventSlug,
                albumId,
                photoId),
            cancellationToken);

        return Ok(
            new FavoritePhotoResponse(
                result.IsFavorited,
                result.FavoriteCount));
    }

    [HttpDelete("{photoId:guid}/favorite")]
    public async Task<ActionResult<FavoritePhotoResponse>> Unfavorite(
        string eventSlug,
        Guid albumId,
        Guid photoId,
        [FromServices] UnfavoritePhotoHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new FavoritePhotoCommand(
                eventSlug,
                albumId,
                photoId),
            cancellationToken);

        return Ok(
            new FavoritePhotoResponse(
                result.IsFavorited,
                result.FavoriteCount));
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<GetFavoritesResponse>> GetFavorites(
        string eventSlug,
        Guid albumId,
        [FromServices] GetFavoritesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetFavoritesQuery(
                eventSlug,
                albumId),
            cancellationToken);

        var items = result.Items
            .Select(item => new GetFavoritesItemResponse(
                item.PhotoId,
                item.FileName,
                item.ThumbnailUrl,
                item.DisplayUrl,
                item.Width,
                item.Height,
                item.CreatedAt))
            .ToList();

        return Ok(new GetFavoritesResponse(items));
    }

    [HttpGet("favorites/download")]
    public async Task<IActionResult> DownloadFavorites(
    string eventSlug,
    Guid albumId,
    [FromServices] DownloadFavoritesHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new DownloadFavoritesQuery(
                eventSlug,
                albumId),
            cancellationToken);

        return File(
            result.Content,
            result.ContentType,
            result.FileName);
    }

    [HttpGet("{photoId:guid}/download")]
    public async Task<IActionResult> DownloadPhoto(
    string eventSlug,
    Guid albumId,
    Guid photoId,
    [FromServices] DownloadPhotoHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new DownloadPhotoQuery(
                eventSlug,
                albumId,
                photoId),
            cancellationToken);

        return File(
            result.Content,
            result.ContentType,
            result.FileName);
    }
}