namespace MutiSaaSApp.Common
{
    /// <summary>
    /// Standard API response wrapper for all endpoints.
    /// Ensures consistent response format across the application.
    /// </summary>
    /// <typeparam name="T">The type of data being returned.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets any errors that occurred.
        /// </summary>
        public Dictionary<string, string>? Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the ApiResponse class for a successful response.
        /// </summary>
        /// <param name="data">The response data.</param>
        /// <param name="message">The response message.</param>
        public ApiResponse(T data, string message = "Success")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the ApiResponse class for a failed response.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errors">Optional dictionary of field-specific errors.</param>
        public ApiResponse(string message, Dictionary<string, string>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
        }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <param name="message">The success message.</param>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>(data, message);
        }

        /// <summary>
        /// Creates an error response.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errors">Optional validation errors.</param>
        public static ApiResponse<T> ErrorResponse(string message, Dictionary<string, string>? errors = null)
        {
            return new ApiResponse<T>(message, errors);
        }
    }
}
