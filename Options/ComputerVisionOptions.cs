namespace Options;

public sealed class ComputerVisionOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public int MaxAllowedSeverity { get; init; } = 1;
}
