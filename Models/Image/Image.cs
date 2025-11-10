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

public enum ImageAccessLevel
{
    Public,
    Private,
    AllowedUsers,
}

public class Image
{
    public int Id { get; set; }

    public string? BlobUri { get; set; } // without SAS
    public string? BlobName { get; set; } // Not null after upload

    public ImageContentType ContentType { get; set; }
    public long Size { get; set; } // Size in bytes

    public ImageMetadata? Metadata { get; set; }
}
