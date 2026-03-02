using Application.DTOs.Invite;
using FluentValidation;

namespace Application.Validators
{
    /// <summary>
    /// Validator for the CreateInviteRequest DTO.
    /// Ensures the email is valid for sending an invite.
    /// </summary>
    public class CreateInviteValidator : AbstractValidator<CreateInviteRequest>
    {
        /// <summary>
        /// Initializes a new instance of the CreateInviteValidator class.
        /// Defines validation rules for creating an invite.
        /// </summary>
        public CreateInviteValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email must be a valid email address.");
        }
    }

    /// <summary>
    /// Validator for the AcceptInviteRequest DTO.
    /// Ensures all required fields for accepting an invite are valid.
    /// </summary>
    public class AcceptInviteValidator : AbstractValidator<AcceptInviteRequest>
    {
        /// <summary>
        /// Initializes a new instance of the AcceptInviteValidator class.
        /// Defines validation rules for accepting an invite.
        /// </summary>
        public AcceptInviteValidator()
        {
            // Token validation
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Invite token is required.");

            // Password validation (same as registration)
            RuleFor(x => x.Password)
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

            // Optional fields
            RuleFor(x => x.FirstName)
                .MaximumLength(256)
                .WithMessage("First name must not exceed 256 characters.")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(256)
                .WithMessage("Last name must not exceed 256 characters.")
                .When(x => !string.IsNullOrEmpty(x.LastName));
        }
    }
}
