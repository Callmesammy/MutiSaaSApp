namespace Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotFoundException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public NotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotFoundException class with entity type and ID.
        /// </summary>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="id">The ID of the entity that was not found.</param>
        public NotFoundException(string entityName, Guid id)
            : base($"{entityName} with id '{id}' was not found.")
        {
        }
    }

    /// <summary>
    /// Exception thrown when a conflict occurs (e.g., duplicate entity).
    /// </summary>
    public class ConflictException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ConflictException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ConflictException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an unauthorized action is attempted.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the UnauthorizedException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the validation errors dictionary.
        /// Key is the field name, value is the error message.
        /// </summary>
        public Dictionary<string, string> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationException class.
        /// </summary>
        /// <param name="errors">Dictionary of validation errors.</param>
        public ValidationException(Dictionary<string, string> errors)
            : base("Validation failed.")
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a single error.
        /// </summary>
        /// <param name="fieldName">The name of the field that failed validation.</param>
        /// <param name="message">The validation error message.</param>
        public ValidationException(string fieldName, string message)
            : base($"Validation failed for {fieldName}.")
        {
            Errors = new Dictionary<string, string> { { fieldName, message } };
        }
    }
}
