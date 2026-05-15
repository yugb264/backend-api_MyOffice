using backend_api.DTOs.Requests;
using FluentValidation;

namespace backend_api.Validators
{
    public class CompareVersionsValidator : AbstractValidator<CompareVersionsRequest>
    {
        public CompareVersionsValidator()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0)
                .WithMessage("Document ID must be a positive number.");

            RuleFor(x => x.V1)
                .GreaterThan(0)
                .WithMessage("Version 1 ID must be a positive number.");

            RuleFor(x => x.V2)
                .GreaterThan(0)
                .WithMessage("Version 2 ID must be a positive number.");

            RuleFor(x => x)
                .Must(x => x.V1 != x.V2)
                .WithName("V2")
                .WithMessage("Cannot compare a version with itself.");
        }
    }
}
