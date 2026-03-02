namespace Application.DTOs.Common
{
    /// <summary>
    /// Base pagination request parameters for list endpoints.
    /// </summary>
    public class PaginationRequest
    {
        /// <summary>
        /// Gets or sets the number of items to skip (default: 0).
        /// </summary>
        public int Skip { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of items to take per page (default: 10, max: 100).
        /// </summary>
        public int Take { get; set; } = 10;

        /// <summary>
        /// Gets or sets the sort order column name.
        /// For tasks: "createdAt" (default), "status", "priority", "dueDate"
        /// </summary>
        public string? SortBy { get; set; } = "createdAt";

        /// <summary>
        /// Gets or sets the sort direction: "asc" or "desc" (default: "desc").
        /// </summary>
        public string? SortDirection { get; set; } = "desc";

        /// <summary>
        /// Validates and normalizes pagination parameters.
        /// </summary>
        public void Normalize()
        {
            Skip = Math.Max(0, Skip);
            Take = Math.Max(1, Math.Min(Take, 100)); // Clamp between 1 and 100
            SortDirection = (SortDirection?.ToLower() == "asc") ? "asc" : "desc";
        }
    }

    /// <summary>
    /// Generic pagination response wrapper for list endpoints.
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated list.</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Gets or sets the list of items in the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the total count of items (across all pages).
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the page size (items per page).
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Gets a value indicating whether there are more pages after the current one.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Gets a value indicating whether there are pages before the current one.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Initializes a new instance of the PaginatedResponse class.
        /// </summary>
        /// <param name="items">The items for the current page.</param>
        /// <param name="totalCount">The total count of all items.</param>
        /// <param name="skip">The number of items skipped (for calculating page number).</param>
        /// <param name="take">The page size.</param>
        public PaginatedResponse(List<T> items, int totalCount, int skip, int take)
        {
            Items = items;
            TotalCount = totalCount;
            PageSize = take;
            PageNumber = (skip / take) + 1;
        }
    }
}
