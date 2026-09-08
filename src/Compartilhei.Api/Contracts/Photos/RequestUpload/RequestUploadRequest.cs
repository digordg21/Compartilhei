namespace Compartilhei.Api.Contracts.Photos.RequestUpload;

public sealed record RequestUploadRequest(
    string FileName,
    long FileSize,
    string ContentType);