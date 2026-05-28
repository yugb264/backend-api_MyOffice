using backend_api.DTOs.Requests;
using FluentValidation;

namespace backend_api.Validators
{
    public class DiffConfigValidator : AbstractValidator<DiffConfigRequest>
    {
        public DiffConfigValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");

            RuleFor(x => x.FileName)
                .Must(fileName => !fileName.Contains("..") && !fileName.Contains("/") && !fileName.Contains("\\"))
                .WithMessage("File name contains invalid characters.")
                .When(x => !string.IsNullOrEmpty(x.FileName));

        }
    }
}
