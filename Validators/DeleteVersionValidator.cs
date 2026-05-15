using backend_api.DTOs.Requests;
using FluentValidation;

namespace backend_api.Validators
{
    public class DeleteVersionValidator : AbstractValidator<DeleteVersionRequest>
    {
        public DeleteVersionValidator()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0)
                .WithMessage("Document ID must be a positive number.");

            RuleFor(x => x.VersionId)
                .GreaterThan(0)
                .WithMessage("Version ID must be a positive number.");
        }
    }
}
