using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Controllers.Image;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services.Image;

namespace hw.Pages.Images.Mine;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IImageService _imageService;
    private readonly UserManager<User> _userManager;

    public IndexModel(
        ILogger<IndexModel> logger,
        IImageMetadataService imageMetadataService,
        IImageService imageService,
        UserManager<User> userManager
    )
    {
        _logger = logger;
        _imageMetadataService = imageMetadataService;
        _imageService = imageService;
        _userManager = userManager;
    }

    public IReadOnlyList<ImageMetadata> Images { get; private set; } = Array.Empty<ImageMetadata>();

    [BindProperty(SupportsGet = true)]
    public FilterParams Filter { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public PaginationParams Pagination { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? SelectedImageId { get; set; }

    public ImageMetadata? SelectedImage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Filter ??= new FilterParams();
        Pagination ??= new PaginationParams();
        Pagination.Size = Pagination.Size <= 0 ? 25 : Pagination.Size;
        Pagination.Skip = Pagination.Skip < 0 ? 0 : Pagination.Skip;

        string? userId = _userManager.GetUserId(User);
        Filter.AuthorId = userId;

        _logger.LogInformation("User ID: {UserId} | Load my images page", userId);
        ViewData["SidebarActiveKey"] = "my-images";

        List<ImageMetadata> images = await _imageMetadataService.GetByFilterAsync(
            userId,
            Filter,
            Pagination,
            cancellationToken
        );

        Images = images
            .Where(metadata => metadata.Image != null)
            .Select(metadata =>
            {
                metadata.Image = _imageService.AttachSASInfo(metadata.Image!);
                return metadata;
            })
            .ToList();

        SelectedImage =
            SelectedImageId.HasValue
                ? Images.FirstOrDefault(metadata => metadata.Id == SelectedImageId.Value)
                : null;

        if (SelectedImage == null && SelectedImageId.HasValue)
        {
            ImageMetadata? explicitSelection = await _imageMetadataService.GetByIdAsync(
                userId,
                SelectedImageId.Value,
                cancellationToken
            );

            if (explicitSelection?.Image != null)
            {
                explicitSelection.Image = _imageService.AttachSASInfo(explicitSelection.Image);
                SelectedImage = explicitSelection;
            }
        }

        ViewData["SelectedImageMetadata"] = SelectedImage;
    }
}
