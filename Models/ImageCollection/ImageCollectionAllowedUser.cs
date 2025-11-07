namespace Models;

public class ImageCollectionAllowedUser
{
    public int Id { get; set; }

    public required int ImageCollectionId { get; set; }
    public ImageCollection? ImageCollection { get; set; }

    public required string UserId { get; set; }
    public User? User { get; set; }
}
