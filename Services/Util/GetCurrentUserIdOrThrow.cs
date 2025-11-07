using Services.Identity;

namespace Services.Util;

public partial class ServiceUtils
{
    public static string GetCurrentUserIdOrThrow(ICurrentUserService currentUserService)
    {
        var userId = currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        return userId;
    }
}