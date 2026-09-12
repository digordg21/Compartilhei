using System.IO.Compression;
using Compartilhei.Application.Abstractions.Compression;

namespace Compartilhei.Infrastructure.Compression;

public sealed class ZipArchiveService : IZipArchiveService
{
    public async Task<Stream> CreateAsync(
        IReadOnlyCollection<ZipArchiveEntrySource> entries,
        CancellationToken cancellationToken)
    {
        var output = new MemoryStream();

        using (var archive = new ZipArchive(
                   output,
                   ZipArchiveMode.Create,
                   leaveOpen: true))
        {
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var zipEntry = archive.CreateEntry(
                    entry.FileName,
                    CompressionLevel.Fastest);

                await using var entryStream =
                    await zipEntry.OpenAsync(cancellationToken);

                await entry.Content.CopyToAsync(
                    entryStream,
                    cancellationToken);
            }
        }

        output.Position = 0;

        return output;
    }
}