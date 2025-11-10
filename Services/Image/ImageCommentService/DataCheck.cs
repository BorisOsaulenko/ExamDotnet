using FluentValidation;
using Models;

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
            .MaximumLength(ImageComment.MaxContentLength)
            .WithMessage($"Comment content must not exceed {ImageComment.MaxContentLength} characters.");

        validator
            .RuleFor(comment => comment.ImageStatsId)
            .GreaterThan(0)
            .WithMessage("ImageStatsId must be a positive integer.");

        validator
            .RuleFor(comment => comment.UserId)
            .NotEmpty()
            .WithMessage("UserId must not be empty.");

        return validator;
    }
}
