namespace Controllers.ImageCollection;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Image;
using Services.ImageCollection;

[ApiController]
[Route("api/image-collection")]
public class ImageCollectionController : ControllerBase
{
    private readonly IImageCollectionService _imageCollectionService;
    private readonly IImageCollectionAllowedUserService _imageCollectionAllowedUserService;
    private readonly IImageMetadataService _imageMetadataService;
    private readonly ILogger<ImageCollectionController> _logger;
    private readonly UserManager<User> _userManager;

    public ImageCollectionController(
        IImageCollectionService imageCollectionService,
        IImageCollectionAllowedUserService imageCollectionAllowedUserService,
        IImageMetadataService imageMetadataService,
        ILogger<ImageCollectionController> logger,
        UserManager<User> userManager
    )
    {
        _imageCollectionService = imageCollectionService;
        _imageCollectionAllowedUserService = imageCollectionAllowedUserService;
        _imageMetadataService = imageMetadataService;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<ImageCollection>> CreateCollection(
        [FromBody] ImageCollectionInput input,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        ImageCollection collection = new ImageCollection
        {
            Title = input.Title,
            Description = input.Description,
            AccessLevel = input.AccessLevel,
            CoverImageMetadataId = input.CoverImageMetadataId,
            UserId = userId,
            AllowedUsers = BuildAllowedUsersPayload(input.AllowedUsers),
            UpdatedAt = DateTime.UtcNow,
        };

        ImageCollection created = await _imageCollectionService.AddAsync(
            collection,
            cancellationToken
        );

        return CreatedAtAction(nameof(GetCollectionById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateCollection(
        int id,
        [FromBody] ImageCollectionInput input,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        ImageCollection collection = new ImageCollection
        {
            Id = id,
            Title = input.Title,
            Description = input.Description,
            AccessLevel = input.AccessLevel,
            CoverImageMetadataId = input.CoverImageMetadataId,
            UserId = userId,
        };

        await _imageCollectionService.UpdateAsync(collection, cancellationToken);

        if (
            input.AllowedUsers != null
            && input.AccessLevel == ImageCollectionAccessLevel.AllowedUsers
            && input.AllowedUsers.Count > 0
        )
        {
            List<ImageCollectionAllowedUser> allowedUsers = BuildAllowedUsersPayload(
                input.AllowedUsers,
                id
            );

            await _imageCollectionAllowedUserService.ReplaceAllowedUsersAsync(
                userId,
                id,
                allowedUsers,
                cancellationToken
            );
        }

        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ImageCollection>> GetCollectionById(
        int id,
        CancellationToken cancellationToken
    )
    {
        string currentUserId = GetUserId() ?? string.Empty;

        List<ImageCollection> collections = await _imageCollectionService.GetByPredicateAsync(
            collection => collection.Id == id,
            currentUserId,
            cancellationToken
        );

        ImageCollection? result = collections.FirstOrDefault();
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ImageCollection>>> GetCollections(
        CancellationToken cancellationToken
    )
    {
        string currentUserId = GetUserId() ?? string.Empty;
        _logger.LogInformation("User ID: {UserId} | Get available collections", currentUserId);

        List<ImageCollection> collections = await _imageCollectionService.GetByPredicateAsync(
            _ => true,
            currentUserId,
            cancellationToken
        );

        return Ok(collections);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteCollection(int id, CancellationToken cancellationToken)
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        ImageCollection toDelete = new ImageCollection
        {
            Id = id,
            UserId = userId,
            Title = string.Empty,
        };
        await _imageMetadataService.RemoveByCollectionIdAsync(userId, id, cancellationToken);
        await _imageCollectionService.RemoveAsync(toDelete, cancellationToken);

        return NoContent();
    }

    [HttpGet("{collectionId:int}/allowed-users")]
    public async Task<ActionResult<List<ImageCollectionAllowedUser>>> GetAllowedUsers(
        int collectionId,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        List<ImageCollectionAllowedUser> allowedUsers =
            await _imageCollectionAllowedUserService.GetByCollectionIdAsync(
                userId,
                collectionId,
                cancellationToken
            );

        return Ok(allowedUsers);
    }

    [HttpPut("{collectionId:int}/allowed-users")]
    public async Task<ActionResult> ReplaceAllowedUsers(
        int collectionId,
        [FromBody] ImageCollectionAllowedUsersInput input,
        CancellationToken cancellationToken
    )
    {
        string? userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        List<ImageCollectionAllowedUser> allowedUsers = BuildAllowedUsersPayload(
            input.UserIds,
            collectionId
        );

        if (allowedUsers.Count == 0)
            return BadRequest("At least one allowed user must be provided.");

        await _imageCollectionAllowedUserService.ReplaceAllowedUsersAsync(
            userId,
            collectionId,
            allowedUsers,
            cancellationToken
        );

        return NoContent();
    }

    private string? GetUserId()
    {
        string? id = _userManager.GetUserId(User);
        return string.IsNullOrEmpty(id) ? null : id;
    }

    private static List<ImageCollectionAllowedUser> BuildAllowedUsersPayload(
        IEnumerable<string>? allowedUserIds,
        int collectionId = 0
    )
    {
        if (allowedUserIds == null)
            return new List<ImageCollectionAllowedUser>();

        return allowedUserIds
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Select(userId => userId.Trim())
            .Distinct(StringComparer.Ordinal)
            .Select(userId => new ImageCollectionAllowedUser
            {
                ImageCollectionId = collectionId,
                UserId = userId,
            })
            .ToList();
    }
}
