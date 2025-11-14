namespace Options;

public sealed class BlobStorageOptions
{
    public string AccountName { get; init; } = string.Empty;
    public string AccountKey { get; init; } = string.Empty;
    public string Container { get; init; } = string.Empty;
}
