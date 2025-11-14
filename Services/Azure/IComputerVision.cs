using System.IO;

namespace Services.Azure;

public interface IComputerVision
{
    /// <summary>
    /// Analyzes the provided image stream and throws if prohibited content is detected.
    /// </summary>
    Task EnsureSafeContentAsync(Stream imageStream, CancellationToken cancellationToken = default);
}
