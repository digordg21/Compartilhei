namespace Compartilhei.Application.Photos.RequestUpload;

public sealed record RequestUploadResult(
    Guid PhotoId,
    string BlobPath,
    string UploadUrl);