using ELearningProject.Features.Shared;
using FluentValidation;

namespace ELearningProject.Features.Batche.CreateBatch
{
    public class CreateBatchValidator : AbstractValidator<CreateBatchCommand>
    {
        public CreateBatchValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Batch name is required.")
                .MaximumLength(100).WithMessage("Batch name must not exceed 100 characters.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Start date must be in the future."); // Optional rule
        }
    }
}
