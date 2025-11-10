using System;
using System.Threading;
using Models;
using ImageModel = Models.Image;

namespace hw.Tests.Services.Image;

internal sealed class TestImageFactory
{
    private int _idSeed;

    public ImageModel Create(Action<ImageModel>? configure = null)
    {
        var image = new ImageModel
        {
            Id = Interlocked.Increment(ref _idSeed),
            UserId = "user123",
            Title = "Test Image",
            Description = "A test image description",
            AccessLevel = ImageAccessLevel.Public,
            ContentType = ImageContentType.Png,
            BlobName = "blob123",
            BlobUri = "http://example.com/blob123",
        };

        configure?.Invoke(image);
        return image;
    }
}
