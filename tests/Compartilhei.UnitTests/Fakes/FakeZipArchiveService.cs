using Compartilhei.Application.Abstractions.Compression;

namespace Compartilhei.UnitTests.Fakes;

public sealed class FakeZipArchiveService : IZipArchiveService
{
    public IReadOnlyCollection<ZipArchiveEntrySource> Entries =>
        _entries.AsReadOnly();

    private readonly List<ZipArchiveEntrySource> _entries = [];

    public Task<Stream> CreateAsync(
        IReadOnlyCollection<ZipArchiveEntrySource> entries,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _entries.Clear();
        _entries.AddRange(entries);

        return Task.FromResult<Stream>(
            new MemoryStream([1, 2, 3]));
    }
}