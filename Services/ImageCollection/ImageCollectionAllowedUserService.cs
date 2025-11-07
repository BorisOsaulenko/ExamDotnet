using Models;
using Repositories;

namespace Services.ImageCollection;

public class ImageCollectionAllowedUserService
    : GenericService<ImageCollectionAllowedUser>, IImageCollectionAllowedUserService
{
    public ImageCollectionAllowedUserService(ImageCollectionAllowedUserRepository repository)
        : base(repository) { }
}
