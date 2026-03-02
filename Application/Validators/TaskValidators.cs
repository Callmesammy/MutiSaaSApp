using Application.DTOs.Task;
using FluentValidation;

namespace Application.Validators
{
    /// <summary>
    /// Validator for the CreateTaskRequest DTO.
    /// </summary>
    public class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
    {
        /// <summary>
        /// Initializes a new instance of the CreateTaskValidator class.
        /// </summary>
        public CreateTaskValidator()
        {
            // Title validation
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Task title is required.")
                .MaximumLength(256)
                .WithMessage("Task title must not exceed 256 characters.");

            // Description validation
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Task description must not exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // Priority validation (enum values are validated by default)

            // DueDate validation
            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Due date must be in the future.")
                .When(x => x.DueDate.HasValue);
        }
    }

    /// <summary>
    /// Validator for the UpdateTaskRequest DTO.
    /// Validates only the fields that are being updated.
    /// </summary>
    public class UpdateTaskValidator : AbstractValidator<UpdateTaskRequest>
    {
        /// <summary>
        /// Initializes a new instance of the UpdateTaskValidator class.
        /// </summary>
        public UpdateTaskValidator()
        {
            // Title validation (optional field)
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Task title cannot be empty.")
                .MaximumLength(256)
                .WithMessage("Task title must not exceed 256 characters.")
                .When(x => !string.IsNullOrEmpty(x.Title));

            // Description validation (optional field)
            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Task description must not exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // DueDate validation (optional field)
            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Due date must be in the future.")
                .When(x => x.DueDate.HasValue);
        }
    }
}
