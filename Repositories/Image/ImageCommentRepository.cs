using Models;

namespace Repositories;

public class ImageCommentRepository
    : GenericRepository<ImageComment>,
        IImageCommentRepository
{
    public ImageCommentRepository(ApplicationDbContext context)
        : base(context) { }
}
