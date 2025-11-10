namespace Models;

public class ImageStats
{
    public int Id { get; set; }

    public ImageMetadata? ImageMetadata { get; set; }
    public int Views { get; set; }
    public int Downloads { get; set; }
    public ICollection<UserPreferences> LikedByUsers { get; set; } = [];
    public int Shares { get; set; }
    public ICollection<ImageComment> ImageComments { get; set; } = [];
}
