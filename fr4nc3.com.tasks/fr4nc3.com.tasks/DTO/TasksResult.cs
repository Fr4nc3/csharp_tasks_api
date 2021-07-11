using fr4nc3.com.tasks.Models;

namespace fr4nc3.com.tasks.DTO
{
    /// <summary>
    /// Defines the public facing task attributes
    /// </summary>
    public class TaskResult : TasksCreatePayload
    {
        public long id { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResult"/> class using a Tasks as input.
        /// </summary>
        /// <param name="tasj">The task.</param>
        public TaskResult(Tasks task)
        {
            id = task.Id ?? -1;
            taskName = task.TaskName;
            isCompleted = task.IsCompleted;
            dueDate = task.DueDate.ToString("yyyy-MM-dd");

        }
    }
    /// <summary>
    /// to returns all the task on db
    /// </summary>
    public class TasksResults
    {
        public TaskResult[] tasks { get; set; }
        /// <summary>
        /// contructor
        /// </summary>
        /// <param name="list"></param>
        public TasksResults(TaskResult[] list)
        {
            tasks = list;
        }
    }

}
