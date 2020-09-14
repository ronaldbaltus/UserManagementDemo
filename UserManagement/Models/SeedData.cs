namespace UserManagement.Models
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Class to insert test data into the database.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Initialize the data seeder.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static void Initialize(IServiceProvider serviceProvider)
        {
#if DEBUG
            InitializeUserData(serviceProvider);
#endif
        }

#if DEBUG
        /// <summary>
        /// Initialize the data seeder.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static void InitializeUserData(IServiceProvider serviceProvider)
        {
            using var userContext = new UserDbContext(serviceProvider.GetRequiredService<DbContextOptions<UserDbContext>>(), serviceProvider.GetRequiredService<EventSourceRepository>());

            userContext.Database.EnsureCreated();

            if (userContext.User.Any())
            {
                // No more seeding of users.
                return;
            }

            userContext.User.AddRange(
                new User()
                {
                    EmailAddress = "ronald.baltus@gmail.com",
                    Password = "somethingyouwontguess",
                },
                new User()
                {
                    EmailAddress = "ronaldbaltus+second@gmail.com",
                    Password = "somethingyouwontguess2",
                }
            );

            userContext.SaveChanges();
        }
#endif
    }
}
