namespace Models;

public class ImageTag
{
    public int Id { get; set; }

    public required int ImageMetadataId { get; set; }
    public required ImageMetadata ImageMetadata { get; set; }

    public required string Tag { get; set; }
}
