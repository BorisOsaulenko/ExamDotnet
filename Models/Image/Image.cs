using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public enum ImageContentType
{
    Png,
    Jpeg,
    Gif,
    Bmp,
    Tiff,
    WebP,
}

public static class ImageContentTypeExtensions
{
    public static string ToMimeType(this ImageContentType contentType)
    {
        return contentType switch
        {
            ImageContentType.Png => "image/png",
            ImageContentType.Jpeg => "image/jpeg",
            ImageContentType.Gif => "image/gif",
            ImageContentType.Bmp => "image/bmp",
            ImageContentType.Tiff => "image/tiff",
            ImageContentType.WebP => "image/webp",
            _ => throw new ArgumentOutOfRangeException(nameof(contentType)),
        };
    }
}

public enum ImageAccessLevel
{
    Public,
    Private,
    AllowedUsers,
}

public class Image
{
    public int Id { get; set; }

    public required string BlobUri { get; set; } // without SAS
    public required string ContainerName { get; set; }
    public required string BlobName { get; set; }

    public ImageContentType ContentType { get; set; }
    public long Size { get; set; } // Size in bytes
    public string? Location { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public ICollection<ImageTag> Tags { get; set; } = [];

    public ImageAccessLevel AccessLevel { get; set; }
    public ICollection<ImageAllowedUser> AllowedUsers { get; set; } = [];

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime EditedAt { get; set; }

    public ICollection<ImageComment> Comments { get; set; } = [];
    public ImageStats? Stats { get; set; }

    public int? ImageCollectionId { get; set; }
    public ImageCollection? ImageCollection { get; set; }

    public required string UserId { get; set; }
    public User? User { get; set; }
}
