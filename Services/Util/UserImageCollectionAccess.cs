using System.Linq.Expressions;
using Models;
using ImageCollectionModel = Models.ImageCollection;

namespace Services.Util;

public partial class ServiceUtils
{
    public class ImageCollection
    {
        public static IQueryable<ImageCollectionModel> ApplyUserFilter(
            IQueryable<ImageCollectionModel> query,
            string userId
        ) =>
            query.Where(collection =>
                collection.AccessLevel == ImageCollectionAccessLevel.Public
                || collection.UserId == userId
                || (
                    collection.AccessLevel == ImageCollectionAccessLevel.AllowedUsers
                    && collection.AllowedUsers.Any(au => au.UserId == userId)
                )
            );

        public static Expression<Func<ImageCollectionModel, bool>> BuildAccessPredicate(
            string userId
        ) =>
            collection =>
                collection.AccessLevel == ImageCollectionAccessLevel.Public
                || collection.UserId == userId
                || (
                    collection.AccessLevel == ImageCollectionAccessLevel.AllowedUsers
                    && collection.AllowedUsers.Any(au => au.UserId == userId)
                );

        public static bool UserHasAccess(ImageCollectionModel collection, string userId)
        {
            if (collection.AccessLevel == ImageCollectionAccessLevel.Public)
            {
                return true;
            }

            if (collection.UserId == userId)
            {
                return true;
            }

            if (collection.AccessLevel == ImageCollectionAccessLevel.AllowedUsers)
            {
                bool isAllowed =
                    collection.AllowedUsers != null
                    && collection.AllowedUsers.Any(au => au.UserId == userId);

                return isAllowed;
            }

            return false;
        }
    }
}
