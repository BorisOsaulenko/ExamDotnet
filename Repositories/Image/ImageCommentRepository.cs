using Models;

namespace Repositories;

public class ImageCommentRepository : GenericRepository<ImageComment>
{
    public ImageCommentRepository(ApplicationDbContext context)
        : base(context) { }
}
