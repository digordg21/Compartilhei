namespace Compartilhei.Application.Photos.ConfirmUpload;

public sealed record ConfirmUploadResult(
    Guid PhotoId,
    string Status);