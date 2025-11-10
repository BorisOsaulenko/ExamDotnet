using Models;

namespace Repositories;

public class ImageMetadataRepository : GenericRepository<ImageMetadata>, IImageMetadataRepository
{
    public ImageMetadataRepository(ApplicationDbContext context)
        : base(context) { }
}
