namespace Compartilhei.Application.Abstractions.Storage;

public sealed record PhotoUploadAuthorization(
    string BlobPath,
    string UploadUrl);