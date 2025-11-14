using Models;

public class SidebarModel
{
    public SidebarModel(string? activeKey = null, ImageMetadata? selectedImageMetadata = null)
    {
        ActiveKey = activeKey;
        SelectedImageMetadata = selectedImageMetadata;
    }

    public string? ActiveKey { get; set; }
    public ImageMetadata? SelectedImageMetadata { get; set; }
}
