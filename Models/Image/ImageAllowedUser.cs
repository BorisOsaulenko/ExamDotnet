using System.Text.Json.Serialization;

namespace Models;

public class ImageAllowedUser
{
    public int Id { get; set; }

    public required int ImageMetadataId { get; set; }

    [JsonIgnore]
    public ImageMetadata? ImageMetadata { get; set; }

    public required string UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}
