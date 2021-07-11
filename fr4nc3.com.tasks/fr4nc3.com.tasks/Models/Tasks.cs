using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fr4nc3.com.tasks.Models
{
    public class Tasks
    {
        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? Id { get; set; }
        /// <summary>
        /// Gets or sets the task's name.
        /// </summary>
        /// <value>The taskame.</value>
        [Required(AllowEmptyStrings = false)]
        [StringLength(100)]
        public string TaskName { get; set; }
        /// <summary>
        /// task complete status 
        /// </summary>
        /// <value>true or false</value>
        [Required]
        public bool IsCompleted { get; set; }
        /// <summary>
        ///  task due date
        /// </summary>
        /// <value>The datetime.</value>
        [Required]
        public DateTime DueDate { get; set; }
    }
}
