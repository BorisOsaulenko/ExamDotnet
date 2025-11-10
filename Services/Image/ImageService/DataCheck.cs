using FluentValidation;
using Models;
using ImageModel = Models.Image;

namespace Services.Image;

public partial class ImageService
{
    public AbstractValidator<ImageModel> CreateImageValidator()
    {
        var validator = new InlineValidator<ImageModel>();
        validator
            .RuleFor(image => image.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.");

        validator
            .RuleFor(image => image.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        validator
            .RuleFor(image => image.BlobUri)
            .NotEmpty()
            .WithMessage("BlobUri is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("BlobUri must be a valid absolute URI.");

        validator.RuleFor(image => image.UserId).NotEmpty().WithMessage("UserId is required.");

        validator
            .RuleFor(image => image.AccessLevel)
            .IsInEnum()
            .WithMessage("AccessLevel must be a valid enum value.");

        validator.RuleFor(image => image.BlobName).NotEmpty().WithMessage("BlobName is required.");

        validator
            .RuleFor(image => image.Tags)
            .NotNull()
            .Must(tags => tags.Count <= 10)
            .WithMessage("A maximum of 10 tags are allowed.");

        validator
            .RuleFor(image => image)
            .Must(image =>
                image.AccessLevel != ImageAccessLevel.AllowedUsers
                || (image.AllowedUsers != null && image.AllowedUsers.Count >= 1)
            )
            .WithMessage(
                "When AccessLevel is 'AllowedUsers', at least one allowed user must be specified."
            );

        return validator;
    }
}
