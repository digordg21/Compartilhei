using Compartilhei.Application.Abstractions.Storage;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakePhotoFileStorage : IPhotoFileStorage
{
    private readonly Dictionary<string, byte[]> _files = [];

    public List<string> OpenedBlobPaths { get; } = [];

    public void Seed(
        string blobPath,
        byte[] content)
    {
        _files[blobPath] = content;
    }

    public Task<Stream> OpenReadAsync(
        string blobPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        OpenedBlobPaths.Add(blobPath);

        if (!_files.TryGetValue(blobPath, out var content))
        {
            throw new FileNotFoundException(
                "Blob not found.",
                blobPath);
        }

        Stream stream = new MemoryStream(
            content,
            writable: false);

        return Task.FromResult(stream);
    }

    public Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}