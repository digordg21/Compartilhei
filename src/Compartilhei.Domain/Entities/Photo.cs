using Compartilhei.Domain.Enums;

namespace Compartilhei.Domain.Entities;

public sealed class Photo
{
    public Guid Id { get; private set; }

    public Guid AlbumId { get; private set; }

    public string FileName { get; private set; } = null!;

    public string? OriginalPath { get; private set; }

    public string? DisplayPath { get; private set; }

    public string? ThumbnailPath { get; private set; }

    public long FileSize { get; private set; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public PhotoStatus Status { get; private set; }

    public Guid UploadedBySessionId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public Album Album { get; private set; } = null!;

    private readonly List<Favorite> _favorites = [];

    public IReadOnlyCollection<Favorite> Favorites =>
        _favorites.AsReadOnly();

    private Photo()
    {
    }

    public Photo(
        Guid albumId,
        string fileName,
        long fileSize,
        Guid uploadedBySessionId)
    {
        if (albumId == Guid.Empty)
        {
            throw new ArgumentException(
                "Album is required.",
                nameof(albumId));
        }

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

        if (uploadedBySessionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Guest session is required.",
                nameof(uploadedBySessionId));
        }

        Id = Guid.NewGuid();
        AlbumId = albumId;
        FileName = fileName.Trim();
        FileSize = fileSize;
        UploadedBySessionId = uploadedBySessionId;
        Status = PhotoStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkUploaded(string originalPath)
    {
        EnsureStatus(PhotoStatus.Pending);

        if (string.IsNullOrWhiteSpace(originalPath))
        {
            throw new ArgumentException(
                "Original path is required.",
                nameof(originalPath));
        }

        OriginalPath = originalPath;
        Status = PhotoStatus.Uploaded;
    }

    public void MarkProcessing()
    {
        EnsureStatus(PhotoStatus.Uploaded);

        Status = PhotoStatus.Processing;
    }

    public void SetDimensions(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                "Height must be greater than zero.");
        }

        Width = width;
        Height = height;
    }

    public void MarkAvailable(
        string displayPath,
        string thumbnailPath)
    {
        EnsureStatus(PhotoStatus.Processing);

        if (!Width.HasValue || !Height.HasValue)
        {
            throw new InvalidOperationException(
                "Photo dimensions must be set before the photo becomes available.");
        }

        if (string.IsNullOrWhiteSpace(displayPath))
        {
            throw new ArgumentException(
                "Display path is required.",
                nameof(displayPath));
        }

        if (string.IsNullOrWhiteSpace(thumbnailPath))
        {
            throw new ArgumentException(
                "Thumbnail path is required.",
                nameof(thumbnailPath));
        }

        DisplayPath = displayPath;
        ThumbnailPath = thumbnailPath;
        Status = PhotoStatus.Available;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed()
    {
        EnsureStatus(PhotoStatus.Processing);

        Status = PhotoStatus.Failed;
    }

    public void RequeueForProcessing()
    {
        EnsureStatus(PhotoStatus.Processing);

        Status = PhotoStatus.Uploaded;
    }

    private void EnsureStatus(PhotoStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException(
                $"Invalid photo status transition. " +
                $"Expected '{expectedStatus}', " +
                $"current status is '{Status}'.");
        }
    }
}