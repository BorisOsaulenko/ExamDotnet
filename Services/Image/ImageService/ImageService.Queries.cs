using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Services.Util;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public async Task<List<ImageModel>> GetWithPaginationAsync(
        int skip,
        int size,
        CancellationToken cancellationToken = default
    )
    {
        var userId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);

        return await ServiceUtils
            .ApplyAccessFilter(_repository.Query(), userId)
            .AsNoTracking()
            .OrderBy(image => image.Id)
            .Skip(skip)
            .Take(size)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ImageModel>> GetByPredicateAsync(
        Expression<Func<ImageModel, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var userId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);

        return await ServiceUtils
            .ApplyAccessFilter(_repository.Query().Where(predicate), userId)
            .ToListAsync(cancellationToken);
    }

    public Task<ImageModel?> GetImageByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var userId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);

        return ServiceUtils
            .ApplyAccessFilter(_repository.Query(), userId)
            .FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
    }
}
