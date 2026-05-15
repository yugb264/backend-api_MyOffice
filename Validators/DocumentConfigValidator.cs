using backend_api.DTOs.Requests;
using FluentValidation;

namespace backend_api.Validators
{
    public class DocumentConfigValidator : AbstractValidator<DocumentConfigRequest>
    {
        public DocumentConfigValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Document ID must be a positive number.");

            RuleFor(x => x.VersionId)
                .GreaterThan(0)
                .WithMessage("Version ID must be a positive number.")
                .When(x => x.VersionId.HasValue);
        }
    }
}
