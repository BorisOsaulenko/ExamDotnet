using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class ImageMetadata
{
    public static readonly int MaxTitleLength = 200;
    public static readonly int MaxLocationLength = 100;
    public static readonly int MaxDescriptionLength = 2000;

    public int Id { get; set; }
    public int ImageId { get; set; }
    public Image? Image { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public ICollection<ImageTag> Tags { get; set; } = [];

    public ImageAccessLevel AccessLevel { get; set; }
    public ICollection<ImageAllowedUser> AllowedUsers { get; set; } = [];

    public int? ImageCollectionId { get; set; }
    public ImageCollection? ImageCollection { get; set; }

    public int ImageStatsId { get; set; }
    public ImageStats? ImageStats { get; set; }

    public required string UserId { get; set; }
    public User? User { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime EditedAt { get; set; }
}
