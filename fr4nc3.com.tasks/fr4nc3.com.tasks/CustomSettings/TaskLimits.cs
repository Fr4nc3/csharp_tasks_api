namespace fr4nc3.com.tasks.CustomSettings
{
    /// <summary>
    /// Defines the task limit settings
    /// </summary>

    public class TaskLimits
    {
        /// Gets or sets the maximum number of tasks.
        /// </summary>
        /// <value>
        /// The maximum number of tasks
        /// </value>
        /// <remarks>Setting a default max task if non provided in config</remarks>
        public int MaxTaskEntrys { get; set; } = 100;
    }
}
