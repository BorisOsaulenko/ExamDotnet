using System.Linq.Expressions;
using Models;
using ImageMetadataModel = Models.ImageMetadata;
using ImageModel = Models.Image;

namespace Services.Image;

public interface IImageService
{
    Task<ImageModel> AddAsync(
        ImageContentType contentType,
        Stream imageStream,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(int imageId, CancellationToken cancellationToken = default);
}
