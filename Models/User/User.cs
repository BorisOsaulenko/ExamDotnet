using Microsoft.AspNetCore.Identity;
namespace Models;

public class User : IdentityUser
{
    public UserPreferences? Preferences { get; set; }
    public ICollection<ImageCollection> ImageCollections { get; set; } = [];

    public ICollection<Image> Images { get; set; } = [];
    public ICollection<ImageComment> Comments { get; set; } = [];

    public ICollection<UserConsumerHistory> ConsumerHistories { get; set; } = [];
    public ICollection<UserProducerHistory> ProducerHistories { get; set; } = [];

    public ICollection<ImageAllowedUser> AllowedImages { get; set; } = [];
    public ICollection<ImageCollectionAllowedUser> AllowedImageCollections { get; set; } = [];

    public ICollection<UserFavoriteTag> FavoriteTags { get; set; } = [];
}
