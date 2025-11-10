using System.Linq.Expressions;
using Models;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.Util;

public partial class ServiceUtils
{
    public class Image
    {
        public static IQueryable<ImageMetadataModel> ApplyAccessFilter(
            IQueryable<ImageMetadataModel> query,
            string userId
        ) => query.Where(BuildAccessPredicate(userId));

        public static bool UserHasAccess(ImageMetadataModel image, string userId)
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

        public static Expression<Func<ImageMetadataModel, bool>> BuildAccessPredicate(
            string userId
        ) =>
            image =>
                image.AccessLevel == ImageAccessLevel.Public
                || image.UserId == userId
                || (
                    image.AccessLevel == ImageAccessLevel.AllowedUsers
                    && image.AllowedUsers.Any(au => au.UserId == userId)
                );
    }
}
