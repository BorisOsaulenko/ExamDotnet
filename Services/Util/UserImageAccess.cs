using System.Linq.Expressions;
using Controllers.Image;
using Models;
using Repositories;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.Util;

public partial class ServiceUtils
{
    public class Image
    {
        public static IQueryable<ImageMetadataModel> ApplyAccessFilter(
            IQueryable<ImageMetadataModel> query,
            string? userId
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
            string? userId
        ) =>
            image =>
                image.AccessLevel == ImageAccessLevel.Public
                || image.UserId == userId
                || (
                    image.AccessLevel == ImageAccessLevel.AllowedUsers
                    && image.AllowedUsers.Any(au => au.UserId == userId)
                );

        public static IQueryable<ImageMetadataModel> ApplyFilter(
            IQueryable<ImageMetadataModel> query,
            FilterParams filterParams,
            IImageStatsRepository imageStatsRepository
        )
        {
            ArgumentNullException.ThrowIfNull(filterParams);
            ArgumentNullException.ThrowIfNull(imageStatsRepository);

            if (!string.IsNullOrWhiteSpace(filterParams.AuthorId))
                query = query.Where(img => img.UserId == filterParams.AuthorId);

            if (filterParams.FromDate != null)
                query = query.Where(img => img.UploadedAt >= filterParams.FromDate);

            if (filterParams.ToDate != null)
                query = query.Where(img => img.UploadedAt <= filterParams.ToDate);

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
                query = query.Where(img =>
                    img.Title.Contains(filterParams.SearchTerm)
                    || (
                        img.Description != null && img.Description.Contains(filterParams.SearchTerm)
                    )
                );

            if (filterParams.Tags is { Count: > 0 })
                query = query.Where(img =>
                    img.Tags.Any(tag => filterParams.Tags.Contains(tag.Tag))
                );

            if (filterParams.SortBy is SortBy sortBy)
            {
                bool descending = filterParams.SortOrder == SortOrder.Descending;

                query = sortBy switch
                {
                    SortBy.Name => descending
                        ? query.OrderByDescending(img => img.Title)
                        : query.OrderBy(img => img.Title),
                    SortBy.Date => descending
                        ? query.OrderByDescending(img => img.UploadedAt)
                        : query.OrderBy(img => img.UploadedAt),
                    _ => ApplyStatsSort(query, sortBy, descending, imageStatsRepository),
                };
            }

            return query;
        }

        private static IQueryable<ImageMetadataModel> ApplyStatsSort(
            IQueryable<ImageMetadataModel> query,
            SortBy sortBy,
            bool descending,
            IImageStatsRepository imageStatsRepository
        )
        {
            var joined = query.Join(
                imageStatsRepository.Query(),
                metadata => metadata.ImageStatsId,
                stats => stats.Id,
                (metadata, stats) => new { Metadata = metadata, Stats = stats }
            );

            return sortBy switch
            {
                SortBy.Views => descending
                    ? joined.OrderByDescending(x => x.Stats.Views).Select(x => x.Metadata)
                    : joined.OrderBy(x => x.Stats.Views).Select(x => x.Metadata),
                SortBy.Downloads => descending
                    ? joined.OrderByDescending(x => x.Stats.Downloads).Select(x => x.Metadata)
                    : joined.OrderBy(x => x.Stats.Downloads).Select(x => x.Metadata),
                SortBy.Shares => descending
                    ? joined.OrderByDescending(x => x.Stats.Shares).Select(x => x.Metadata)
                    : joined.OrderBy(x => x.Stats.Shares).Select(x => x.Metadata),
                SortBy.Likes => descending
                    ? joined
                        .OrderByDescending(x => x.Stats.LikedByUsers.Count)
                        .Select(x => x.Metadata)
                    : joined.OrderBy(x => x.Stats.LikedByUsers.Count).Select(x => x.Metadata),
                _ => query,
            };
        }
    }
}
