using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UserManagement
{
    /// <summary>
    /// User Context handler to handle all user data related actions.
    /// </summary>
    public class UserDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDbContext"/> class.
        /// Construct User Context to handle all user data related actions.
        /// </summary>
        /// <param name="options">Options for connecting to the database.</param>
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Userdata accessor.
        /// </summary>
        public DbSet<Models.User> User { get; set; }

        /// <summary>
        /// Override the save changes to hook in event sourcing.
        /// </summary>
        /// <returns>Number of changed entries.</returns>
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries();

            StoreEvents(entries.Where(e => e.Entity is User)).GetAwaiter().GetResult();

            return base.SaveChanges();
        }

        /// <summary>
        /// Override the save changes to hook in event sourcing.
        /// </summary>
        /// <returns>Number of changed entries.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();
            await StoreEvents(entries.Where(e => e.Entity is User));

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Check the changes of a model and add events that are part of the user.
        /// </summary>
        /// <returns>The Task.</returns>
        private async Task StoreEvents(IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries)
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
                    if ((entry.State == EntityState.Added || property.IsModified) && !property.IsTemporary)
                    {
                        user.RuntimeEvents.Add(new Event()
                        {
                            Type = entry.State == EntityState.Added ? Event.EventType.Create : Event.EventType.Update,
                            Fieldname = property.Metadata.Name,
                            PreviousValue = entry.State == EntityState.Added ? null : dbValues.GetValue<object>(property.Metadata.Name).ToString(),
                            NewValue = property.CurrentValue.ToString(),
                        });
                    }
                }
            }
        }
    }
}
