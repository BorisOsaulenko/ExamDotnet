using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Options;

public sealed class BlobContainerClients
{
    public BlobContainerClients(BlobServiceClient serviceClient, IOptions<BlobStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentNullException.ThrowIfNull(options);

        var value = options.Value;
        Public = serviceClient.GetBlobContainerClient(value.PublicContainer);
        Private = serviceClient.GetBlobContainerClient(value.PrivateContainer);
    }

    public BlobContainerClient Public { get; }
    public BlobContainerClient Private { get; }
}
