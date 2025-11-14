using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Models;

public class ImageComment
{
    public static readonly int MaxContentLength = 1000;

    public int Id { get; set; }

    public int ImageStatsId { get; set; }

    [JsonIgnore]
    public ImageStats? ImageStats { get; set; }

    public required string UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    public required string Content { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }
}
