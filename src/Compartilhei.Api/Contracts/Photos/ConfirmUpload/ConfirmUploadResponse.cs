namespace Compartilhei.Api.Contracts.Photos.ConfirmUpload;

public sealed record ConfirmUploadResponse(
    Guid PhotoId,
    string Status);