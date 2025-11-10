using FluentValidation;
using ImageCollectionModel = Models.ImageCollection;

namespace Services.ImageCollection;

public partial class ImageCollectionService
{
    public AbstractValidator<ImageCollectionModel> CreateImageCollectionValidator()
    {
        var validator = new InlineValidator<ImageCollectionModel>();
        validator
            .RuleFor(ic => ic.Title)
            .NotEmpty()
            .WithMessage("Image collection title must not be empty.")
            .MaximumLength(ImageCollectionModel.MaxTitleLength)
            .WithMessage(
                $"Image collection title must not exceed {ImageCollectionModel.MaxTitleLength} characters."
            );

        validator
            .RuleFor(ic => ic.Description)
            .MaximumLength(ImageCollectionModel.MaxDescriptionLength)
            .WithMessage(
                $"Image collection description must not exceed {ImageCollectionModel.MaxDescriptionLength} characters."
            );

        validator
            .RuleFor(ic => ic.AccessLevel)
            .IsInEnum()
            .WithMessage("Invalid access level for image collection.");

        validator.RuleFor(ic => ic.UserId).NotEmpty().WithMessage("UserId must not be empty.");

        validator
            .RuleFor(ic => ic.CoverImageMetadataId)
            .GreaterThan(0)
            .When(ic => ic.CoverImageMetadataId.HasValue)
            .WithMessage("CoverImageMetadataId must be greater than 0 if specified.");

        return validator;
    }
}
