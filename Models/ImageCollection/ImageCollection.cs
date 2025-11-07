using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public enum ImageCollectionAccessLevel
{
    Public,
    AllowedUsers,
    Private,
}

public class ImageCollection
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public ICollection<Image> Images { get; set; } = [];

    public ImageCollectionAccessLevel AccessLevel { get; set; }
    public ICollection<ImageCollectionAllowedUser> AllowedUsers { get; set; } = [];

    public int? CoverImageId { get; set; }
    public Image? CoverImage { get; set; }

    public ICollection<UserPreferences> Subscribers { get; set; } = [];

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

    public required string UserId { get; set; }
    public User? User { get; set; }
}
