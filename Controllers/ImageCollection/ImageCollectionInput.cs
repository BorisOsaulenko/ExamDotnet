namespace Controllers.ImageCollection;

using Models;

public class ImageCollectionInput
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ImageCollectionAccessLevel AccessLevel { get; set; } = ImageCollectionAccessLevel.Public;
    public int? CoverImageMetadataId { get; set; }
    public List<string>? AllowedUsers { get; set; }
}

public class ImageCollectionAllowedUsersInput
{
    public List<string> UserIds { get; set; } = new();
}
