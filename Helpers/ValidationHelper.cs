using backend_api.DTOs.Common;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace backend_api.Helpers
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Converts a FluentValidation result into a standardized API error response.
        /// Returns null if validation passed (so the caller can continue to business logic).
        /// </summary>
        public static IActionResult? ToErrorResponse(ValidationResult result)
        {
            if (result.IsValid)
                return null;

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new ApiResponse<Dictionary<string, string[]>>
            {
                Success = false,
                Message = "Validation failed",
                Data = errors
            });
        }
    }
}
