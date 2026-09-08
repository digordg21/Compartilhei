using Compartilhei.Application.Abstractions.Processing;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Application.Photos.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Compartilhei.Infrastructure.Images;

public sealed class ImageSharpPhotoProcessor : IPhotoProcessor
{
    private const int DisplayMaxSize = 2000;
    private const int ThumbnailMaxSize = 400;

    private readonly IPhotoFileStorage _storage;

    public ImageSharpPhotoProcessor(IPhotoFileStorage storage)
    {
        _storage = storage;
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
                        DisplayMaxSize,
                        DisplayMaxSize),
                    Mode = ResizeMode.Max
                });
            });

        await using var displayStream = new MemoryStream();

        await displayImage.SaveAsync(
            displayStream,
            new JpegEncoder
            {
                Quality = 85
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
                        ThumbnailMaxSize,
                        ThumbnailMaxSize),
                    Mode = ResizeMode.Max
                });
            });

        await using var thumbnailStream = new MemoryStream();

        await thumbnailImage.SaveAsync(
            thumbnailStream,
            new JpegEncoder
            {
                Quality = 80
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