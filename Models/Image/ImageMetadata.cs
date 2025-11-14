using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Models;

public class ImageMetadata
{
    public static readonly int MaxTitleLength = 200;
    public static readonly int MaxLocationLength = 100;
    public static readonly int MaxDescriptionLength = 2000;

    public int Id { get; set; }
    public int ImageId { get; set; }
    public Image? Image { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public ICollection<ImageTag> Tags { get; set; } = [];

    public ImageAccessLevel AccessLevel { get; set; }
    public ICollection<ImageAllowedUser> AllowedUsers { get; set; } = [];

    public int? ImageCollectionId { get; set; }

    [JsonIgnore]
    public ImageCollection? ImageCollection { get; set; }

    public int ImageStatsId { get; set; }
    public ImageStats? ImageStats { get; set; }

    public string? UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime EditedAt { get; set; } = DateTime.UtcNow;
}
