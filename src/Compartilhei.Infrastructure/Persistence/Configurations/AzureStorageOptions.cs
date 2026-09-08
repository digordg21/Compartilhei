namespace Compartilhei.Infrastructure.Configuration;

public sealed class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    public string ConnectionString { get; init; } = string.Empty;

    public string ContainerName { get; init; } = string.Empty;

    public int UploadSasExpirationMinutes { get; init; } = 15;

    public int ReadSasExpirationMinutes { get; init; } = 10;
}