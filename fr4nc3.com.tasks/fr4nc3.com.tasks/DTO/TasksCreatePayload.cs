using fr4nc3.com.tasks.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace fr4nc3.com.tasks.DTO
{
    [CustomValidation(typeof(TasksCreatePayload), "ValidateTaskName")]
    public class TasksCreatePayload
    {
        /// <summary>
        /// Gets or sets the task's name.
        /// </summary>
        /// <value>The taskame.</value>
        [Required(ErrorMessage = "3")]
        [StringLength(100, ErrorMessage = "2")]
        public string taskName { get; set; }
        /// <summary>
        /// task complete status 
        /// </summary>
        /// <value>true or false</value>
        [Required(ErrorMessage = "3")]
        public bool isCompleted { get; set; }
        /// <summary>
        ///  task due date
        /// </summary>
        /// <value>The datetime.</value>
        [Required(ErrorMessage = "3")]
        [RegularExpression(@"\d{4}-\d{2}-\d{2}", ErrorMessage = "7")]
        public string dueDate { get; set; }

        /// <summary>
        /// Validates the taskName.
        /// </summary>
        /// <param name="task">The TasksCreatePayload.</param>
        /// <param name="ctx">The context which contains the actual value from the field that the task validator is validating.</param>
        /// <returns>ValidationResult.</returns>
        public static ValidationResult ValidateTaskName(TasksCreatePayload task, ValidationContext ctx)
        {
            // validate the taskname
            if (task.taskName == null || task.taskName.Length <= 0)
            {
                return new ValidationResult($"{ErrorCode.ParameterTooSmall}", new List<string> { "taskName" });
            }
            if (task.taskName.Length > 100)
            {
                return new ValidationResult($"{ErrorCode.ParameterTooLarge}", new List<string> { "taskName" });
            }

            return ValidationResult.Success;
        }

    }
}
