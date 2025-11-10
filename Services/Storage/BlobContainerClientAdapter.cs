using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Services.Storage;

public sealed class BlobContainerClientAdapter : IBlobContainerClient
{
    private readonly BlobContainerClient _inner;

    public BlobContainerClientAdapter(BlobContainerClient inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public bool CanGenerateSasUri => _inner.CanGenerateSasUri;

    public string Name => _inner.Name;

    public Task UploadAsync(
        string blobName,
        Stream content,
        BlobHttpHeaders blobHttpHeaders,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(blobHttpHeaders);

        return _inner
            .GetBlobClient(blobName)
            .UploadAsync(content, blobHttpHeaders, cancellationToken: cancellationToken);
    }

    public Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName);

        return _inner
            .GetBlobClient(blobName)
            .DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public Uri GenerateSasUri(string blobName, BlobSasBuilder sasBuilder)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName);
        ArgumentNullException.ThrowIfNull(sasBuilder);

        return _inner.GetBlobClient(blobName).GenerateSasUri(sasBuilder);
    }

    public Uri GetBlobUri(string blobName)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName);
        return _inner.GetBlobClient(blobName).Uri;
    }
}
