using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators
{
    /// <summary>
    /// Validator for the RegisterOrganizationRequest DTO.
    /// Ensures all required fields meet business rules.
    /// </summary>
    public class RegisterOrganizationValidator : AbstractValidator<RegisterOrganizationRequest>
    {
        /// <summary>
        /// Initializes a new instance of the RegisterOrganizationValidator class.
        /// Defines all validation rules for organization registration.
        /// </summary>
        public RegisterOrganizationValidator()
        {
            // Organization Name validation
            RuleFor(x => x.OrganizationName)
                .NotEmpty()
                .WithMessage("Organization name is required.")
                .MinimumLength(2)
                .WithMessage("Organization name must be at least 2 characters.")
                .MaximumLength(256)
                .WithMessage("Organization name must not exceed 256 characters.");

            // Email validation
            RuleFor(x => x.AdminEmail)
                .NotEmpty()
                .WithMessage("Admin email is required.")
                .EmailAddress()
                .WithMessage("Admin email must be a valid email address.");

            // Password validation
            RuleFor(x => x.AdminPassword)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters.")
                .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one digit.")
                .Matches(@"[!@#$%^&*]")
                .WithMessage("Password must contain at least one special character (!@#$%^&*).");
        }
    }
}
