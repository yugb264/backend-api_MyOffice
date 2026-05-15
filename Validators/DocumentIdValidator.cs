using backend_api.DTOs.Requests;
using FluentValidation;

namespace backend_api.Validators
{
    public class DocumentIdValidator : AbstractValidator<DocumentIdRequest>
    {
        public DocumentIdValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID must be a positive number.");
        }
    }
}
