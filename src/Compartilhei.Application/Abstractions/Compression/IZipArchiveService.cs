namespace Compartilhei.Application.Abstractions.Compression;

public interface IZipArchiveService
{
    Task<Stream> CreateAsync(
        IReadOnlyCollection<ZipArchiveEntrySource> entries,
        CancellationToken cancellationToken);
}

public sealed record ZipArchiveEntrySource(
    string FileName,
    Stream Content);