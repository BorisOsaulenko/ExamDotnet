using Models;

namespace Repositories;

public class ImageRepository : GenericRepository<Image>
{
    public ImageRepository(ApplicationDbContext context)
        : base(context) { }
}
