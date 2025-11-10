using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Services.Storage;

public interface IBlobContainerClient
{
    bool CanGenerateSasUri { get; }
    string Name { get; }

    Task UploadAsync(
        string blobName,
        Stream content,
        BlobHttpHeaders blobHttpHeaders,
        CancellationToken cancellationToken = default
    );

    Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken = default);

    Uri GenerateSasUri(string blobName, BlobSasBuilder sasBuilder);
    Uri GetBlobUri(string blobName);
}
