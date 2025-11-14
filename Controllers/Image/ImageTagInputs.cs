namespace Controllers.Image;

public class ImageTagInput
{
    public string Tag { get; set; } = string.Empty;
}

public class ImageTagListInput
{
    public List<string> Tags { get; set; } = new();
}

public class ImageAllowedUserInput
{
    public string UserId { get; set; } = string.Empty;
}

public class ImageAllowedUserListInput
{
    public List<string> UserIds { get; set; } = new();
}
