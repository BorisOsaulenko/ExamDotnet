using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Services.Util;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task<List<ImageModel>> GetWithPaginationAsync(
        string userId,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        List<ImageModel> imagesNoSAS = await ServiceUtils
            .Image.ApplyAccessFilter(_repository.Query(), userId)
            .AsNoTracking()
            .OrderBy(image => image.Id)
            .Skip(pagination.Skip)
            .Take(pagination.Size)
            .ToListAsync(cancellationToken);

        return imagesNoSAS.Select(image => AttachSASInfo(image, userId)).ToList();
    }

    public async Task<List<ImageModel>> GetByPredicateAsync(
        string userId,
        Expression<Func<ImageModel, bool>> predicate,
        PaginationParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ImageModel> query = ServiceUtils
            .Image.ApplyAccessFilter(_repository.Query().Where(predicate), userId)
            .AsNoTracking()
            .OrderBy(image => image.Id)
            .Skip(pagination.Skip)
            .Take(pagination.Size);

        List<ImageModel> imagesNoSAS = await query.ToListAsync(cancellationToken);

        return imagesNoSAS.Select(image => AttachSASInfo(image, userId)).ToList();
    }

    public async Task<ImageModel?> GetImageByIdAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default
    )
    {
        ImageModel? imageNoSAS = await ServiceUtils
            .Image.ApplyAccessFilter(_repository.Query(), userId)
            .FirstOrDefaultAsync(image => image.Id == id, cancellationToken);

        return imageNoSAS == null ? null : AttachSASInfo(imageNoSAS, userId);
    }
}
