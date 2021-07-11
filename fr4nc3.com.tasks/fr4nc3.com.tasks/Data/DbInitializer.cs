using fr4nc3.com.tasks.Models;
using System;
using System.Linq;


namespace fr4nc3.com.tasks.Data
{
    /// <summary>
    /// Initializes (seeds) the database with data
    /// </summary>
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the specified context with data
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Initialize(MyDatabaseContext context)
        {
            // Check to see if there is any data in the task table
            if (context.Tasks.Any())
            {
                // Tasks table has data, nothing to do here
                return;
            }
            // add default entries
            Tasks[] tasks = new Tasks[]
            {
                new Tasks() { TaskName = "Buy groceries", IsCompleted = false, DueDate =  DateTime.Parse("2021-02-02")},
                new Tasks() { TaskName = "Workout", IsCompleted= true, DueDate =  DateTime.Parse("2021-01-01")},
                new Tasks() { TaskName = "Paint fence", IsCompleted= false, DueDate =  DateTime.Parse("2021-03-15")},
                new Tasks() { TaskName = "Mow Lawn", IsCompleted= false, DueDate =  DateTime.Parse("2021-06-11")},
            };

            foreach (Tasks task in tasks)
            {
                context.Tasks.Add(task);
            }

            // Commit the changes to the database
            context.SaveChanges();

        }
    }
}
