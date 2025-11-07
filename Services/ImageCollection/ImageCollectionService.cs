using ImageCollectionModel = Models.ImageCollection;
using Repositories;

namespace Services.ImageCollection;

public class ImageCollectionService : GenericService<ImageCollectionModel>, IImageCollectionService
{
    public ImageCollectionService(ImageCollectionRepository repository)
        : base(repository) { }
}
