using Compartilhei.Application.Abstractions.Storage;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakePhotoStorage : IPhotoStorage
{
    public bool Called { get; private set; }

    public Guid EventId { get; private set; }

    public Guid AlbumId { get; private set; }

    public Guid PhotoId { get; private set; }

    public string? FileName { get; private set; }

    public string? ContentType { get; private set; }

    public bool BlobExists { get; set; }

    public string? LastBlobPathChecked { get; private set; }

    public List<string> ReadBlobPaths { get; } = [];

    public Task<PhotoUploadAuthorization> CreateUploadAuthorizationAsync(
        Guid eventId,
        Guid albumId,
        Guid photoId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        Called = true;

        EventId = eventId;
        AlbumId = albumId;
        PhotoId = photoId;
        FileName = fileName;
        ContentType = contentType;

        var extension = Path.GetExtension(fileName);

        var blobPath =
            $"events/{eventId}/albums/{albumId}/original/{photoId}{extension}";

        return Task.FromResult(
            new PhotoUploadAuthorization(
                blobPath,
                $"https://fake-storage/{blobPath}"));
    }

    public Task<bool> ExistsAsync(
        string blobPath,
        CancellationToken cancellationToken)
    {
        LastBlobPathChecked = blobPath;

        return Task.FromResult(BlobExists);
    }

    public Task<string> CreateReadUrlAsync(
    string blobPath,
    CancellationToken cancellationToken)
    {
        ReadBlobPaths.Add(blobPath);

        return Task.FromResult(
            $"https://fake-storage/read/{blobPath}");
    }
}