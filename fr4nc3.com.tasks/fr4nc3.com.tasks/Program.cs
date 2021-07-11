using fr4nc3.com.tasks.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace fr4nc3.com.tasks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            // Get the dependency injection for creating services
            using (var scope = host.Services.CreateScope())
            {
                // Get the service provider so services can be called
                var services = scope.ServiceProvider;
                try
                {
                    // Get the database context service
                    var context = services.GetRequiredService<MyDatabaseContext>();

                    // Initialize the data
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "*** ERROR *** An error occurred while seeding the database. *** ERROR ***");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
