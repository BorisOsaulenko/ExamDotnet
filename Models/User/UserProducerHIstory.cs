namespace Models;

public enum UserHistoryType
{
    Image,
    Collection,
}

public sealed record UserHistoryKey(UserHistoryType HistoryType, int? HistoryId);

public enum ProducerActivityType
{
    ImageUpload,
    ImageEdit,
    ImageDelete,
    CollectionCreate,
    CollectionEdit,
    CollectionDelete,
}

public class UserProducerHistory
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }

    public ProducerActivityType ActivityType { get; set; }
    public DateTime ActivityDate { get; set; }

    // Details about the activity: ImageEdit
    public int? ImageId { get; set; }
    public string? PreviousImageDescription { get; set; }
    public string? PreviousImageTitle { get; set; }
    public string? PreviousImageTags { get; set; }
    public string? PreviousImageLocation { get; set; }
    public ImageAccessLevel? PreviousImageAccessLevel { get; set; }

    // Details about the activity: CollectionEdit
    public int? CollectionId { get; set; }
    public string? PreviousCollectionTitle { get; set; }
    public string? PreviousCollectionDescription { get; set; }
    public int? PreviousCoverImageId { get; set; }
    public ImageAccessLevel? PreviousCollectionAccessLevel { get; set; }
}
