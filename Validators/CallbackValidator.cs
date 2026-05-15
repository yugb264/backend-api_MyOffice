using backend_api.DTOs.Requests;
using FluentValidation;

namespace backend_api.Validators
{
    public class CallbackValidator : AbstractValidator<CallbackRequest>
    {
        public CallbackValidator()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0)
                .WithMessage("Document ID must be a positive number.");

            RuleFor(x => x.Data)
                .NotNull()
                .WithMessage("Callback data is required.");

            RuleFor(x => x.Data)
                .Must(data => data["status"] != null)
                .WithMessage("Callback data must contain a 'status' field.")
                .When(x => x.Data != null);
        }
    }
}
