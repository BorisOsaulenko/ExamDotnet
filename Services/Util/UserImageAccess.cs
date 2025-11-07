using System.Linq.Expressions;
using Models;
using ImageModel = Models.Image;

namespace Services.Util;

public partial class ServiceUtils
{
    public static IQueryable<ImageModel> ApplyAccessFilter(
        IQueryable<ImageModel> query,
        string userId
    ) =>
        query.Where(image =>
            image.AccessLevel == ImageAccessLevel.Public
            || image.UserId == userId
            || (
                image.AccessLevel == ImageAccessLevel.AllowedUsers
                && image.AllowedUsers.Any(au => au.UserId == userId)
            )
        );

    public static bool UserHasAccess(ImageModel image, string userId)
    {
        if (image.AccessLevel == ImageAccessLevel.Public)
        {
            return true;
        }

        if (image.UserId == userId)
        {
            return true;
        }

        if (image.AccessLevel == ImageAccessLevel.AllowedUsers)
        {
            bool isAllowed =
                image.AllowedUsers != null && image.AllowedUsers.Any(au => au.UserId == userId);

            return isAllowed;
        }

        return false;
    }

    public static Expression<Func<ImageModel, bool>> BuildAccessPredicate(string userId) =>
        image =>
            image.AccessLevel == ImageAccessLevel.Public
            || image.UserId == userId
            || (
                image.AccessLevel == ImageAccessLevel.AllowedUsers
                && image.AllowedUsers.Any(au => au.UserId == userId)
            );
}
