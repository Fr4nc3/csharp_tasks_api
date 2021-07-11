using fr4nc3.com.tasks.Models;
using Microsoft.EntityFrameworkCore;
namespace fr4nc3.com.tasks.Data
{
    public class MyDatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyDatabaseContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public MyDatabaseContext(DbContextOptions<MyDatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// Represents the tasks table (Entity Set)
        /// </summary>
        /// <value>
        /// The tasks.
        /// </value>
        public DbSet<Tasks> Tasks { get; set; }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Adds the task to tne entity model linking it to the Tasks table
            modelBuilder.Entity<Tasks>().ToTable("Tasks");
            modelBuilder.Entity<Tasks>().HasIndex(p => p.Id).IsUnique();
            modelBuilder.Entity<Tasks>().HasIndex(p => p.TaskName).IsUnique();

        }
    }
}
