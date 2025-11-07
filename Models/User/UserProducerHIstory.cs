namespace Models;

public enum ProducerActivityType
{
    Signup,
    Login,
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

    // Details about the activity (null if not changed): ImageEdit
    public string? PreviousImageDescription { get; set; }
    public string? PreviousImageTitle { get; set; }
    public string? PreviousImageTags { get; set; }
    public ImageAccessLevel? PreviousImageAccessLevel { get; set; }

    // Details about the activity (null if not changed): CollectionEdit
    public string? PreviousCollectionTitle { get; set; }
    public string? PreviousCollectionDescription { get; set; }
}
