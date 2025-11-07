using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class ImageComment
{
    public int Id { get; set; }

    public int ImageId { get; set; }
    public Image? Image { get; set; }


    public required string UserId { get; set; }
    public User? User { get; set; }

    public required string Content { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }
}
