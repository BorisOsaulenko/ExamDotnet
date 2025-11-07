namespace Models;

public class ImageAllowedUser
{
    public int Id { get; set; }

    public required int ImageId { get; set; }
    public Image? Image { get; set; }

    public required string UserId { get; set; }
    public User? User { get; set; }
}
