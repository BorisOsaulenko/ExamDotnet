using FluentValidation;
using Models;
using ImageModel = Models.ImageMetadata;

namespace Services.Image;

public partial class ImageMetadataService
{
    public AbstractValidator<ImageModel> CreateImageValidator()
    {
        var validator = new InlineValidator<ImageModel>();
        validator
            .RuleFor(image => image.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(ImageMetadata.MaxTitleLength)
            .WithMessage($"Title cannot exceed {ImageMetadata.MaxTitleLength} characters.");

        validator
            .RuleFor(image => image.Description)
            .MaximumLength(ImageMetadata.MaxDescriptionLength)
            .WithMessage($"Description cannot exceed {ImageMetadata.MaxDescriptionLength} characters.");

        validator.RuleFor(image => image.UserId).NotEmpty().WithMessage("UserId is required.");

        validator
            .RuleFor(image => image.AccessLevel)
            .IsInEnum()
            .WithMessage("AccessLevel must be a valid enum value.");

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
