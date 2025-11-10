namespace Models;

public class ImageAllowedUser
{
    public int Id { get; set; }

    public required int ImageMetadataId { get; set; }
    public ImageMetadata? Image { get; set; }

    public required string UserId { get; set; }
    public User? User { get; set; }
}
