using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Models;
using Services.Image;
using Services.ImageCollection;

namespace hw.Pages.ImageCollection.Mine;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IImageCollectionService _imageCollectionService;
    private readonly IImageService _imageService;
    private readonly UserManager<User> _userManager;

    public IndexModel(
        ILogger<IndexModel> logger,
        IImageCollectionService imageCollectionService,
        IImageService imageService,
        UserManager<User> userManager
    )
    {
        _logger = logger;
        _imageCollectionService = imageCollectionService;
        _imageService = imageService;
        _userManager = userManager;
    }

    public IReadOnlyList<Models.ImageCollection> Collections { get; private set; } =
        Array.Empty<Models.ImageCollection>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        string? userId = _userManager.GetUserId(User);
        ViewData["SidebarActiveKey"] = "my-collections";

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Anonymous user attempted to access My Collections page.");
            Collections = Array.Empty<Models.ImageCollection>();
            return;
        }

        _logger.LogInformation("User ID: {UserId} | Load my collections page", userId);

        List<Models.ImageCollection> collections = await _imageCollectionService.GetByPredicateAsync(
            collection => collection.UserId == userId,
            userId,
            cancellationToken
        );

        foreach (Models.ImageCollection collection in collections)
        {
            if (collection.CoverImageMetadata?.Image != null)
            {
                collection.CoverImageMetadata.Image = _imageService.AttachSASInfo(
                    collection.CoverImageMetadata.Image
                );
            }
            else if (collection.Images != null && collection.Images.Count > 0)
            {
                foreach (ImageMetadata metadata in collection.Images)
                {
                    if (metadata.Image != null)
                    {
                        metadata.Image = _imageService.AttachSASInfo(metadata.Image);
                    }
                }
            }
        }

        Collections = collections;
    }
}
