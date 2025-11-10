using FluentValidation;
using Models;
using ImageCollectionModel = Models.ImageCollection;
using ImageMetadataModel = Models.ImageMetadata;

namespace Services.User;

public partial class UserProducerHistoryService
{
    public AbstractValidator<UserProducerHistory> CreateUserHistoryValidator()
    {
        var validator = new InlineValidator<UserProducerHistory>();
        validator
            .RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty.")
            .MaximumLength(100)
            .WithMessage("UserId cannot exceed 100 characters.");
        validator
            .RuleFor(x => x.ActivityDate)
            .NotEmpty()
            .WithMessage("ActivityDate cannot be empty.");
        validator
            .RuleFor(x => x.ActivityType)
            .IsInEnum()
            .WithMessage("ActivityType must be a valid enum value.");
        validator
            .RuleFor(x => x.PreviousCollectionAccessLevel)
            .IsInEnum()
            .WithMessage("PreviousCollectionAccessLevel must be a valid enum value.")
            .NotNull()
            .When(x => x.ActivityType == ProducerActivityType.CollectionEdit);
        validator
            .RuleFor(x => x.PreviousCollectionDescription)
            .MaximumLength(ImageCollectionModel.MaxDescriptionLength)
            .WithMessage(
                $"PreviousCollectionDescription cannot exceed {ImageCollectionModel.MaxDescriptionLength} characters."
            );
        validator
            .RuleFor(x => x.PreviousCollectionTitle)
            .MaximumLength(ImageCollectionModel.MaxTitleLength)
            .WithMessage(
                $"PreviousCollectionTitle cannot exceed {ImageCollectionModel.MaxTitleLength} characters."
            )
            .NotNull()
            .When(x => x.ActivityType == ProducerActivityType.CollectionEdit);
        validator
            .RuleFor(x => x.CollectionId)
            .GreaterThan(0)
            .WithMessage("CollectionId must be a positive integer.")
            .NotNull()
            .When(x =>
                new List<ProducerActivityType>
                {
                    ProducerActivityType.CollectionCreate,
                    ProducerActivityType.CollectionEdit,
                    ProducerActivityType.CollectionDelete,
                }.Contains(x.ActivityType)
            );

        validator
            .RuleFor(x => x.PreviousImageAccessLevel)
            .IsInEnum()
            .WithMessage("PreviousImageAccessLevel must be a valid enum value.")
            .NotNull()
            .When(x => x.ActivityType == ProducerActivityType.ImageEdit);
        validator
            .RuleFor(x => x.PreviousImageDescription)
            .MaximumLength(ImageMetadataModel.MaxDescriptionLength)
            .WithMessage(
                $"PreviousImageDescription cannot exceed {ImageMetadataModel.MaxDescriptionLength} characters."
            );
        validator
            .RuleFor(x => x.PreviousImageTitle)
            .MaximumLength(ImageMetadataModel.MaxTitleLength)
            .WithMessage(
                $"PreviousImageTitle cannot exceed {ImageMetadataModel.MaxTitleLength} characters."
            )
            .NotNull()
            .When(x => x.ActivityType == ProducerActivityType.ImageEdit);
        validator
            .RuleFor(x => x.PreviousImageTags)
            .NotNull()
            .When(x => x.ActivityType == ProducerActivityType.ImageEdit);
        validator
            .RuleFor(x => x.PreviousImageLocation)
            .MaximumLength(ImageMetadataModel.MaxLocationLength)
            .WithMessage(
                $"PreviousImageLocation cannot exceed {ImageMetadataModel.MaxLocationLength} characters."
            );
        validator
            .RuleFor(x => x.ImageId)
            .GreaterThan(0)
            .WithMessage("ImageId must be a positive integer.")
            .NotNull()
            .When(x =>
                new List<ProducerActivityType>
                {
                    ProducerActivityType.ImageUpload,
                    ProducerActivityType.ImageEdit,
                    ProducerActivityType.ImageDelete,
                }.Contains(x.ActivityType)
            );

        validator
            .RuleFor(x => x)
            .Must(x => !(x.CollectionId.HasValue && x.ImageId.HasValue))
            .WithMessage("Only one of CollectionId or ImageId can be set.");
        return validator;
    }
}
