using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebServer.Services
{
    /// <summary>
    /// The background task service let us do tasks in the background.
    /// </summary>
    public class BackgroundTaskService : IHostedService
    {
        private readonly ILogger<BackgroundTaskService> logger;
        private Timer timer;
        private IServiceProvider services;

        /// <summary>
        /// Construct the background task service
        /// </summary>
        /// <param name="logger">The logger scoped to this service.</param>
        /// <param name="services">Services provider.</param>
        public BackgroundTaskService(ILogger<BackgroundTaskService> logger, IServiceProvider services)
        {
            this.logger = logger;
            this.services = services;
        }

        /// <summary>
        /// Start the Backround service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Awaitable task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Timed BackgroundTaskService running.");

            timer = new Timer(RunTasks, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        /// <summary>
        /// The actual process that runs each period.
        /// </summary>
        /// <param name="state">The state of the timer.</param>
        public void RunTasks(object state)
        {
            logger.LogInformation("Removing scheduled users..");

            using (var scope = services.CreateScope())
            {
                var userDbContext = scope.ServiceProvider.GetRequiredService<UserManagement.UserDbContext>();

                userDbContext.PermanentlyRemoveScheduledUsers().Wait();
            }
        }

        /// <summary>
        /// Stops the Backround service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Awaitable task</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Timed BackgroundTaskService is stopping.");
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
