using FluentValidation;

namespace ELearningProject.Features.Batche.UpdateBatch
{
    public class UpdateBatchValidator : AbstractValidator<UpdateBatchCommand>
    {
        public UpdateBatchValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Batch ID is required.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Batch name is required.")
                .MaximumLength(100).WithMessage("Batch name must not exceed 100 characters.");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
        }
    }
}
