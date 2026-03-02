namespace Domain.Enums
{
    /// <summary>
    /// Defines the possible status values for a task.
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Task is not yet started.
        /// </summary>
        Todo = 1,

        /// <summary>
        /// Task is currently in progress.
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Task is completed.
        /// </summary>
        Done = 3
    }
}
