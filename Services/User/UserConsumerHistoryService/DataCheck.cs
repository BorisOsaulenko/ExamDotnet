using FluentValidation;
using Models;

namespace Services.User;

public partial class UserConsumerHistoryService
{
    public AbstractValidator<UserConsumerHistory> CreateUserHistoryValidator()
    {
        var validator = new InlineValidator<UserConsumerHistory>();

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
            .RuleFor(x => x.ImageId)
            .GreaterThan(0)
            .WithMessage("ImageId must be a positive integer.")
            .NotNull()
            .When(x =>
                new List<ConsumerActivityType>
                {
                    ConsumerActivityType.ImageDownload,
                    ConsumerActivityType.ImageShare,
                }.Contains(x.ActivityType)
            );

        validator
            .RuleFor(x => x.CollectionId)
            .GreaterThan(0)
            .WithMessage("CollectionId must be a positive integer.")
            .NotNull()
            .When(x =>
                new List<ConsumerActivityType>
                {
                    ConsumerActivityType.CollectionView,
                    ConsumerActivityType.CollectionDownload,
                    ConsumerActivityType.CollectionShare,
                }.Contains(x.ActivityType)
            );

        validator
            .RuleFor(x => x)
            .Must(x => !(x.CollectionId.HasValue && x.ImageId.HasValue))
            .WithMessage("Only one of CollectionId or ImageId can be set.");

        return validator;
    }
}
