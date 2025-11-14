using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Options;

public sealed class BlobContainerClients
{
    public BlobContainerClients(
        BlobServiceClient serviceClient,
        IOptions<BlobStorageOptions> options
    )
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentNullException.ThrowIfNull(options);

        var value = options.Value;
        Container = serviceClient.GetBlobContainerClient(value.Container);
    }

    public BlobContainerClient Container { get; }
}
