using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Photos.Processing;
using Compartilhei.Infrastructure.Images.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Options;

namespace Compartilhei.Infrastructure.Images;

public sealed class ImageSharpPhotoProcessor : IPhotoProcessor
{

    private readonly IPhotoFileStorage _storage;
    private readonly ImageProcessingOptions _options;

    public ImageSharpPhotoProcessor(
        IPhotoFileStorage storage,
        IOptions<ImageProcessingOptions> options)
    {
        _storage = storage;
        _options = options.Value;
    }

    public async Task<PhotoProcessingResult> ProcessAsync(
        PhotoProcessingRequest request,
        CancellationToken cancellationToken)
    {
        await using var originalStream =
            await _storage.OpenReadAsync(
                request.OriginalPath,
                cancellationToken);

        using var image =
            await Image.LoadAsync(
                originalStream,
                cancellationToken);

        image.Mutate(x => x.AutoOrient());

        var width = image.Width;
        var height = image.Height;

        using var displayImage =
            image.Clone(context =>
            {
                context.Resize(new ResizeOptions
                {
                    Size = new Size(
                        _options.DisplayMaxWidth,
                        _options.DisplayMaxHeight),
                    Mode = ResizeMode.Max
                });
            });

        await using var displayStream = new MemoryStream();

        await displayImage.SaveAsync(
            displayStream,
            new JpegEncoder
            {
                Quality = _options.DisplayQuality
            },
            cancellationToken);

        displayStream.Position = 0;

        await _storage.UploadAsync(
            request.DisplayPath,
            displayStream,
            "image/jpeg",
            cancellationToken);

        using var thumbnailImage =
            image.Clone(context =>
            {
                context.Resize(new ResizeOptions
                {
                    Size = new Size(
                        _options.ThumbnailMaxWidth,
                        _options.ThumbnailMaxHeight),
                    Mode = ResizeMode.Max
                });
            });

        await using var thumbnailStream = new MemoryStream();

        await thumbnailImage.SaveAsync(
            thumbnailStream,
            new JpegEncoder
            {
                Quality = _options.ThumbnailQuality,
            },
            cancellationToken);

        thumbnailStream.Position = 0;

        await _storage.UploadAsync(
            request.ThumbnailPath,
            thumbnailStream,
            "image/jpeg",
            cancellationToken);

        return new PhotoProcessingResult(
            request.DisplayPath,
            request.ThumbnailPath,
            width,
            height);
            }
}