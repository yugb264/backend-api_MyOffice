using backend_api.DTOs.Requests;
using FluentValidation;

namespace backend_api.Validators
{
    public class UploadDocumentValidator : AbstractValidator<UploadDocumentRequest>
    {
        private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

        public UploadDocumentValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required.");

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File is empty.")
                .When(x => x.File != null);

            RuleFor(x => x.File.Length)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage("File size must not exceed 25 MB.")
                .When(x => x.File != null);

        }
    }
}
