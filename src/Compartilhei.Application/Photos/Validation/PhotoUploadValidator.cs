namespace Compartilhei.Application.Photos.Validation;

public sealed class PhotoUploadValidator
{
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public void Validate(
        string fileName,
        long fileSize,
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));
        }

        if (fileSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileSize),
                "File size must be greater than zero.");
        }

        if (fileSize > MaxFileSizeBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileSize),
                $"File size cannot exceed {MaxFileSizeBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException(
                "Content type is required.",
                nameof(contentType));
        }

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedExtensions.Contains(extension.ToLowerInvariant()))
        {
            throw new ArgumentException(
                "File extension is not supported.",
                nameof(fileName));
        }

        if (!AllowedContentTypes.Contains(
                contentType.ToLowerInvariant()))
        {
            throw new ArgumentException(
                "Content type is not supported.",
                nameof(contentType));
        }
    }
}