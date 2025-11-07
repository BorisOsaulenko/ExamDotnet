namespace Models;

public class ImageStats
{
    public int Id { get; set; }
    public int ImageId { get; set; }
    public Image? Image { get; set; }
    public int Views { get; set; }
    public int Downloads { get; set; }
    public int Likes { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
}
