namespace Models;

public class ImageTag
{
    public int Id { get; set; }

    public required int ImageId { get; set; }
    public required Image Image { get; set; }

    public required string Tag { get; set; }
}
