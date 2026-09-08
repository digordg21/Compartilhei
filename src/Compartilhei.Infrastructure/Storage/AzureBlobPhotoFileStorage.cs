using Azure.Storage.Blobs;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Compartilhei.Infrastructure.Storage;

public sealed class AzureBlobPhotoFileStorage : IPhotoFileStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageOptions _options;

    public AzureBlobPhotoFileStorage(
        BlobServiceClient blobServiceClient,
        IOptions<AzureStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
    }

    public async Task<Stream> OpenReadAsync(
        string blobPath,
        CancellationToken cancellationToken)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient(
                _options.ContainerName);

        var blobClient =
            containerClient.GetBlobClient(blobPath);

        return await blobClient.OpenReadAsync(
            cancellationToken: cancellationToken);
    }

    public async Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient(
                _options.ContainerName);

        var blobClient =
            containerClient.GetBlobClient(blobPath);

        await blobClient.UploadAsync(
            content,
            overwrite: true,
            cancellationToken);

        await blobClient.SetHttpHeadersAsync(
            new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = contentType
            },
            cancellationToken: cancellationToken);
    }
}