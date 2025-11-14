using System.Collections.Generic;
using System.IO;
using Azure;
using Azure.AI.ContentSafety;
using Microsoft.Extensions.Options;
using Options;

namespace Services.Azure;

public sealed class ComputerVisionService : IComputerVision
{
    private static readonly IReadOnlyList<ImageCategory> CategoriesToCheck =
    [
        ImageCategory.Sexual,
        ImageCategory.Violence,
        ImageCategory.Hate,
        ImageCategory.SelfHarm,
    ];

    private readonly ContentSafetyClient _client;
    private readonly ComputerVisionOptions _options;

    public ComputerVisionService(
        ContentSafetyClient client,
        IOptions<ComputerVisionOptions> optionsAccessor
    )
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
    }

    public async Task EnsureSafeContentAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        if (!imageStream.CanRead)
        {
            throw new ArgumentException("Image stream must be readable.", nameof(imageStream));
        }

        var imageData = new ContentSafetyImageData(BinaryData.FromStream(imageStream));
        var analyzeOptions = new AnalyzeImageOptions(imageData);

        foreach (var category in CategoriesToCheck)
        {
            analyzeOptions.Categories.Add(category);
        }

        Response<AnalyzeImageResult> response = await _client
            .AnalyzeImageAsync(analyzeOptions, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<ImageCategoriesAnalysis>? analyses = response.Value.CategoriesAnalysis;

        if (analyses == null || analyses.Count == 0)
        {
            return;
        }

        foreach (ImageCategoriesAnalysis analysis in analyses)
        {
            if (analysis?.Severity is int severity && severity > _options.MaxAllowedSeverity)
            {
                throw new InvalidOperationException(
                    $"Image contains prohibited {analysis.Category} content (severity {severity})."
                );
            }
        }
    }
}
