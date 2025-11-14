namespace Controllers.Image;

using Models;

public class ImageMetadataInput
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public ImageAccessLevel AccessLevel { get; set; } = ImageAccessLevel.Public;
    public List<string> AllowedUsers { get; set; } = new List<string>();
    public List<string> Tags { get; set; } = new List<string>();
    public int? ImageCollectionId { get; set; }
}