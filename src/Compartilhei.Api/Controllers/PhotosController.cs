using Compartilhei.Api.Contracts.Photos.RequestUpload;
using Compartilhei.Api.Contracts.Photos.ConfirmUpload;
using Compartilhei.Api.Contracts.Photos.Gallery;
using Compartilhei.Application.Photos.Gallery;
using Compartilhei.Application.Photos.RequestUpload;
using Compartilhei.Application.Photos.ConfirmUpload;

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
                item.CreatedAt))
            .ToList();

        return Ok(new PhotoGalleryResponse(
            items,
            result.NextCursor,
            result.HasMore));
    }
}