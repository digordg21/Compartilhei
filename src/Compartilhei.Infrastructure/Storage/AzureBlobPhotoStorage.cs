using System.Globalization;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Compartilhei.Application.Abstractions.Storage;
using Compartilhei.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Compartilhei.Infrastructure.Storage;

public sealed class AzureBlobPhotoStorage : IPhotoStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageOptions _options;

    public AzureBlobPhotoStorage(
        BlobServiceClient blobServiceClient,
        IOptions<AzureStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
    }

    public Task<PhotoUploadAuthorization> CreateUploadAuthorizationAsync(
        Guid eventId,
        Guid albumId,
        Guid photoId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new InvalidOperationException(
                "O arquivo precisa possuir uma extensão válida.");
        }

        var blobPath =
            $"events/{eventId}/albums/{albumId}/original/{photoId}{extension.ToLowerInvariant()}";

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        var blobClient =
            containerClient.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(
                _options.UploadSasExpirationMinutes)
        };

        sasBuilder.SetPermissions(
            BlobSasPermissions.Create |
            BlobSasPermissions.Write);

        var uploadUrl = blobClient.GenerateSasUri(sasBuilder);

        var authorization = new PhotoUploadAuthorization(
            blobPath,
            uploadUrl.ToString());

        return Task.FromResult(authorization);
    }

    public async Task<bool> ExistsAsync(
    string blobPath,
    CancellationToken cancellationToken)
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient(
                _options.ContainerName);

        var blobClient =
            containerClient.GetBlobClient(blobPath);

        var response = await blobClient.ExistsAsync(
            cancellationToken);

        return response.Value;
    }

    public Task<string> CreateReadUrlAsync(
    string blobPath,
    CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(blobPath))
        {
            throw new ArgumentException(
                "Blob path is required.",
                nameof(blobPath));
        }

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(
                _options.ContainerName);

        var blobClient =
            containerClient.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(
                _options.ReadSasExpirationMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return Task.FromResult(
            blobClient.GenerateSasUri(sasBuilder).ToString());
    }
}