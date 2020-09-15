namespace UserManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using UserManagement.Models;

    /// <summary>
    /// User Context handler to handle all user data related actions.
    /// </summary>
    public class UserDbContext : DbContext
    {
        private readonly EventSourceRepository eventSourceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDbContext"/> class.
        /// Construct User Context to handle all user data related actions.
        /// </summary>
        /// <param name="options">Options for connecting to the database.</param>
        /// <param name="eventSourceRepository">Repository to store events.</param>
        public UserDbContext(DbContextOptions<UserDbContext> options, EventSourceRepository eventSourceRepository)
            : base(options)
        {
            this.eventSourceRepository = eventSourceRepository;
        }

        /// <summary>
        /// Gets or sets the Userdata accessor.
        /// </summary>
        public DbSet<User> User { get; set; }

        /// <summary>
        /// Override the save changes to hook in event sourcing.
        /// </summary>
        /// <returns>Number of changed entries.</returns>
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries();
            var userEntries = entries.Where(e => e.Entity is User);
            UpdateUserEvents(userEntries).GetAwaiter().GetResult();

            // Gather all users before saving.
            var users = userEntries.Select(ue => ue.Entity).Cast<User>().ToArray();
            var result = base.SaveChanges();

            var tasks = users.Select(u => eventSourceRepository.StoreEvents(u, u.RuntimeEvents));
            Task.WhenAll(tasks).Wait();

            return result;
        }

        /// <summary>
        /// Override the save changes to hook in event sourcing.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of changed entries.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();
            var userEntries = entries.Where(e => e.Entity is User);
            await UpdateUserEvents(userEntries);

            // Gather all users before saving.
            var users = userEntries.Select(ue => ue.Entity).Cast<User>().ToArray();
            var result = await base.SaveChangesAsync(cancellationToken);

            // Store the events.
            var tasks = users.Select(u => eventSourceRepository.StoreEvents(u, u.RuntimeEvents));
            await Task.WhenAll(tasks);

            // Let ef core save.
            return result;
        }

        /// <summary>
        /// Updates the user in the database.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>Awaitable task.</returns>
        public async Task UpdateUser(User user)
        {
            Attach(user).State = EntityState.Modified;

            await SaveChangesAsync();
        }

        /// <summary>
        /// Schedule the user for remove.
        /// </summary>
        /// <param name="user">The user that should be removed.</param>
        /// <returns>Awaitable task.</returns>
        public Task ScheduleUserForRemove(User user)
        {
            user.RemovedAt = DateTime.UtcNow;

            return UpdateUser(user);
        }

        /// <summary>
        /// Checks whether the user ID exists in the database.
        /// </summary>
        /// <param name="id">ID of the user.</param>
        /// <returns>Boolean containing whether the user-id was found.</returns>
        public bool UserExists(int id)
        {
            return User.Any(e => e.ID == id);
        }

        /// <summary>
        /// Lists all users in the database.
        /// </summary>
        /// <returns>List of users.</returns>
        public IQueryable<User> ListUsers()
        {
            return User.Where(u => u.RemovedAt == null);
        }

        /// <summary>
        /// Permanently delete users that are scheduled for deletion
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task PermanentlyRemoveScheduledUsers()
        {
#if DEBUG
            var removableUsers = await User.Where(u => u.RemovedAt < DateTime.UtcNow.AddMinutes(-2)).ToListAsync();
#else
            var removableUsers = await User.Where(u => u.RemovedAt < DateTime.UtcNow.AddDays(-30)).ToListAsync();
#endif

            Console.WriteLine($"Removing {removableUsers.Count} users.");

            User.RemoveRange(removableUsers);

            await SaveChangesAsync();

            // Remove related events.
            removableUsers.ForEach(u => eventSourceRepository.RemoveEvents(u));
        }

        /// <summary>
        /// Check the changes of a model and add events that are part of the user.
        /// </summary>
        /// <returns>The Task.</returns>
        private async Task UpdateUserEvents(IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (!(entry.Entity is User user))
                {
                    continue;
                }

                var dbValues = await entry.GetDatabaseValuesAsync();

                foreach (var property in entry.Properties)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                        case EntityState.Deleted:
                            user.RuntimeEvents.Add(new Event()
                            {
                                Type = entry.State == EntityState.Added ? Event.EventType.Create : Event.EventType.Delete,
                                Fieldname = property.Metadata.Name,
                                NewValue = property.CurrentValue?.ToString() ?? null,
                            });
                            break;
                        case EntityState.Modified:
                            var prevValue = dbValues.GetValue<object>(property.Metadata.Name)?.ToString() ?? null;
                            var curValue = property.CurrentValue?.ToString() ?? null;

                            if (prevValue != curValue)
                            {
                                user.RuntimeEvents.Add(new Event()
                                {
                                    Type = Event.EventType.Update,
                                    Fieldname = property.Metadata.Name,
                                    PreviousValue = prevValue,
                                    NewValue = curValue,
                                });
                            }

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Override the table definition for our users.
        /// </summary>
        /// <param name="modelBuilder">The builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>(entity =>
                {
                    entity.HasIndex(e => e.EmailAddress).IsUnique();
                });
        }
    }
}
