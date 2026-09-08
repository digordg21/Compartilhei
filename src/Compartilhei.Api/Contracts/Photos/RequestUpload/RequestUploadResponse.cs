namespace Compartilhei.Api.Contracts.Photos.RequestUpload;

public sealed record RequestUploadResponse(
    Guid PhotoId,
    string BlobPath,
    string UploadUrl);