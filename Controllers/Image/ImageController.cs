namespace Controllers.Image;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Image;
using Services.ImageCollection;
using ImageMetadataModel = Models.ImageMetadata;

[ApiController]
[Route("api/image")]
public class ImageController : ControllerBase
{
    private readonly IImageMetadataService _imageMetadataService;
    private readonly IImageService _imageService;
    private readonly IImageStatsService _imageStatsService;
    private readonly IImageCommentService _imageCommentService;
    private readonly IImageCollectionService _imageCollectionService;
    private readonly IImageTagService _imageTagService;
    private readonly IImageAllowedUserService _imageAllowedUserService;
    private readonly ILogger<ImageController> _logger;
    private readonly UserManager<User> _userManager;

    public ImageController(
        IImageMetadataService imageMetadataService,
        IImageService imageService,
        IImageStatsService imageStatsService,
        IImageCommentService imageCommentService,
        IImageCollectionService imageCollectionService,
        IImageTagService imageTagService,
        IImageAllowedUserService imageAllowedUserService,
        ILogger<ImageController> logger,
        UserManager<User> userManager
    )
    {
        _imageMetadataService = imageMetadataService;
        _imageService = imageService;
        _logger = logger;
        _imageStatsService = imageStatsService;
        _imageCommentService = imageCommentService;
        _imageCollectionService = imageCollectionService;
        _imageTagService = imageTagService;
        _imageAllowedUserService = imageAllowedUserService;
        _userManager = userManager;
    }

    private string? GetUserId()
    {
        var id = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(id))
            return null;
        return id;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImageMetadataModel>> UploadImage(
        [FromForm] IFormFile imageFile,
        [FromForm] string metadataInputJson,
        CancellationToken cancellationToken
    )
    {
        ImageMetadataInput metadataInput =
            System.Text.Json.JsonSerializer.Deserialize<ImageMetadataInput>(metadataInputJson)
            ?? throw new ArgumentException("Invalid metadata JSON.");

        if (imageFile == null || imageFile.Length == 0)
            return BadRequest("No image file provided.");

        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        ImageContentType? contentType = ImageService.ParseFromFileName(imageFile.FileName);
        if (contentType == null)
            return BadRequest("Unsupported image format.");

        using var stream = imageFile.OpenReadStream();

        Image image = await _imageService.AddAsync(contentType.Value, stream, cancellationToken);

        ImageMetadataModel metadata = new ImageMetadataModel
        {
            Title = metadataInput.Title,
            Description = metadataInput.Description,
            UserId = userId,
            ImageId = image.Id,
            ImageCollectionId = metadataInput.ImageCollectionId,
            AccessLevel = metadataInput.AccessLevel,
            AllowedUsers = BuildAllowedUsersPayload(metadataInput.AllowedUsers),
            Tags = metadataInput
                .Tags.Select(tag => new ImageTag { Tag = tag, ImageMetadataId = 0 })
                .ToList(),
        };

        ImageMetadataModel createdMetadata = await _imageMetadataService.AddAsync(
            metadata,
            cancellationToken
        );

        return Ok(createdMetadata);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateImageMetadata(
        int id,
        [FromBody] ImageMetadataModel metadata,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        metadata.Id = id;
        metadata.UserId = userId;

        await _imageMetadataService.UpdateAsync(metadata, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ImageMetadataModel>> GetImageById(
        int id,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        _logger.LogInformation("User ID: {UserId} | Get image by uid", userId);
        ImageMetadataModel? metadata = await _imageMetadataService.GetByIdAsync(
            userId,
            id,
            cancellationToken
        );

        if (metadata == null || metadata.Image == null)
            return NotFound();

        await _imageStatsService.IncrementViewsAsync(id, cancellationToken);
        metadata.Image = _imageService.AttachSASInfo(metadata.Image);

        return Ok(metadata);
    }

    [HttpGet]
    public async Task<ActionResult<List<ImageMetadataModel>>> GetAvailableImages(
        [FromQuery] FilterParams? filterParams,
        [FromQuery] PaginationParams? paginationParams,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        _logger.LogInformation("User ID: {UserId} | Get available images", userId);

        List<ImageMetadataModel> images = await _imageMetadataService.GetByFilterAsync(
            userId,
            filterParams ?? new FilterParams(),
            paginationParams ?? new PaginationParams(),
            cancellationToken
        );

        images = images
            .Where(metadata => metadata.Image != null)
            .Select(metadata =>
            {
                metadata.Image = _imageService.AttachSASInfo(metadata.Image!);
                return metadata;
            })
            .ToList();

        return Ok(images);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteImage(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        ImageMetadataModel? metadataToDelete = new ImageMetadataModel { Id = id, UserId = userId };

        await _imageMetadataService.RemoveAsync(metadataToDelete, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:int}/likes/toggle")]
    public async Task<ActionResult> ToggleLike(int id, CancellationToken cancellationToken)
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        ImageMetadataModel? metadata = await _imageMetadataService
            .GetByIdAsync(userId, id, cancellationToken)
            .ConfigureAwait(false);

        if (metadata == null)
            return NotFound();

        bool liked = await _imageStatsService
            .ToggleLikeAsync(id, userId, cancellationToken)
            .ConfigureAwait(false);

        return Ok(new { liked });
    }

    [HttpPost("{id:int}/tags")]
    public async Task<ActionResult<ImageTag>> AddTag(
        int id,
        [FromBody] ImageTagInput request,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || string.IsNullOrWhiteSpace(request.Tag))
            return BadRequest("Tag value is required.");

        ImageTag createdTag = await _imageTagService.AddAsync(
            userId,
            new ImageTag { ImageMetadataId = id, Tag = request.Tag.Trim() },
            cancellationToken
        );

        return Ok(createdTag);
    }

    [HttpDelete("{id:int}/tags")]
    public async Task<ActionResult> RemoveTag(
        int id,
        [FromQuery] string tag,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(tag))
            return BadRequest("Tag value is required.");

        await _imageTagService.RemoveAsync(
            userId,
            new ImageTag { ImageMetadataId = id, Tag = tag.Trim() },
            cancellationToken
        );

        return NoContent();
    }

    [HttpPut("{id:int}/tags")]
    public async Task<ActionResult<List<ImageTag>>> ReplaceTags(
        int id,
        [FromBody] ImageTagListInput request,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        List<ImageTag> tags = BuildImageTagsPayload(request?.Tags, id);

        List<ImageTag> result = await _imageTagService.ReplaceAsync(
            userId,
            id,
            tags,
            cancellationToken
        );

        return Ok(result);
    }

    [HttpPost("{id:int}/allowed-users")]
    public async Task<ActionResult<ImageAllowedUser>> AddAllowedUser(
        int id,
        [FromBody] ImageAllowedUserInput request,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest("UserId is required.");

        ImageAllowedUser created = await _imageAllowedUserService.AddAsync(
            userId,
            new ImageAllowedUser { ImageMetadataId = id, UserId = request.UserId.Trim() },
            cancellationToken
        );

        return Ok(created);
    }

    [HttpDelete("{id:int}/allowed-users/{allowedUserId:int}")]
    public async Task<ActionResult> RemoveAllowedUser(
        int id,
        int allowedUserId,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        await _imageAllowedUserService.RemoveAsync(
            userId,
            new ImageAllowedUser
            {
                Id = allowedUserId,
                ImageMetadataId = id,
                UserId = userId,
            },
            cancellationToken
        );

        return NoContent();
    }

    [HttpPut("{id:int}/allowed-users")]
    public async Task<ActionResult<List<ImageAllowedUser>>> ReplaceAllowedUsers(
        int id,
        [FromBody] ImageAllowedUserListInput request,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        List<ImageAllowedUser> allowedUsers = BuildAllowedUsersPayload(request?.UserIds, id);

        List<ImageAllowedUser> result = await _imageAllowedUserService.ReplaceAsync(
            userId,
            id,
            allowedUsers,
            cancellationToken
        );

        return Ok(result);
    }

    private static List<ImageAllowedUser> BuildAllowedUsersPayload(
        IEnumerable<string>? allowedUserIds,
        int imageMetadataId = 0
    )
    {
        if (allowedUserIds == null)
            return new List<ImageAllowedUser>();

        return allowedUserIds
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Select(userId => userId.Trim())
            .Distinct(StringComparer.Ordinal)
            .Select(userId => new ImageAllowedUser
            {
                ImageMetadataId = imageMetadataId,
                UserId = userId,
            })
            .ToList();
    }

    private static List<ImageTag> BuildImageTagsPayload(
        IEnumerable<string>? tags,
        int imageMetadataId
    )
    {
        if (tags == null)
            return new List<ImageTag>();

        return tags.Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(tag => new ImageTag { ImageMetadataId = imageMetadataId, Tag = tag })
            .ToList();
    }
}
