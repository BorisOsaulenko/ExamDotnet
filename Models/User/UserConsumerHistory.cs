namespace Models;

public enum ConsumerActivityType
{
    ImageDownload,
    ImageShare,
    CollectionView,
    CollectionDownload,
    CollectionShare,
}

public class UserConsumerHistory
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }

    public ConsumerActivityType ActivityType { get; set; }
    public DateTime ActivityDate { get; set; }

    // Optional fields for ImageDownload and ImageShare
    public int? ImageId { get; set; }
    public Image? Image { get; set; }

    // Optional fields for CollectionView, CollectionDownload and CollectionShare
    public int? CollectionId { get; set; }
    public ImageCollection? Collection { get; set; }
}
