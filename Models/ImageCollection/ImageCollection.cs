using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Models;

public enum ImageCollectionAccessLevel
{
    Public,
    AllowedUsers,
    Private,
}

public class ImageCollection
{
    public static readonly int MaxTitleLength = 200;
    public static readonly int MaxDescriptionLength = 2000;

    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public ICollection<ImageMetadata> Images { get; set; } = [];

    public ImageCollectionAccessLevel AccessLevel { get; set; }
    public ICollection<ImageCollectionAllowedUser> AllowedUsers { get; set; } = [];

    public int? CoverImageMetadataId { get; set; }
    public ImageMetadata? CoverImageMetadata { get; set; }

    public ICollection<UserPreferences> Subscribers { get; set; } = [];

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public required string UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}
