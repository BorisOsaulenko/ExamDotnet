using FluentValidation;
using Models;
using Services.Util;

namespace Services.Image;

public partial class ImageCommentService
{
    public AbstractValidator<ImageComment> CreateImageCommentValidator()
    {
        var validator = new InlineValidator<ImageComment>();
        validator
            .RuleFor(comment => comment.Content)
            .NotEmpty()
            .WithMessage("Comment content must not be empty.")
            .MaximumLength(1000)
            .WithMessage("Comment content must not exceed 1000 characters.");

        validator
            .RuleFor(comment => comment.ImageId)
            .GreaterThan(0)
            .WithMessage("ImageId must be a positive integer.");

        validator
            .RuleFor(comment => comment.UserId)
            .NotEmpty()
            .WithMessage("UserId must not be empty.")
            .Must(userId =>
            {
                string currentUserId = ServiceUtils.GetCurrentUserIdOrThrow(_currentUserService);
                return userId == currentUserId;
            });

        return validator;
    }
}
