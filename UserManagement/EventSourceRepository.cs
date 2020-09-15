namespace UserManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EventStore.ClientAPI;
    using Newtonsoft.Json;

    /// <summary>
    /// Get access to the event store data.
    /// </summary>
    public class EventSourceRepository
    {
        private readonly IEventStoreConnection eventStoreConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceRepository"/> class.
        /// </summary>
        /// <param name="eventStoreConnection">Connection to the event store database.</param>
        public EventSourceRepository(IEventStoreConnection eventStoreConnection)
        {
            this.eventStoreConnection = eventStoreConnection;
        }

        /// <summary>
        /// Store the given events for this user in the events stream.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="events">The events.</param>
        /// <returns>The storable task.</returns>
        public Task StoreEvents(Models.User user, IEnumerable<Models.Event> events)
        {
            return StoreEvents($"user-{user.ID}", events);
        }

        /// <summary>
        /// Store the given events in the events stream.
        /// </summary>
        /// <param name="key">Append the events under this key.</param>
        /// <param name="events">The events to store.</param>
        /// <returns>Task.</returns>
        public Task StoreEvents(string key, IEnumerable<Models.Event> events)
        {
            return eventStoreConnection.AppendToStreamAsync(
                key,
                ExpectedVersion.Any,
                events.Select(e =>
                {
                    return new EventData(
                        Guid.NewGuid(),
                        e.Type.ToString(),
                        true,
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e)),
                        Encoding.UTF8.GetBytes("{}")
                    );
                }).ToArray()
            );
        }

        /// <summary>
        /// Get the given events for this user in the events stream.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="count">The number of events to retrieve.</param>
        /// <returns>Awaitable task for the events.</returns>
        public Task<IEnumerable<Models.RecordedEvent>> GetEvents(Models.User user, int count = 100)
        {
            return GetEvents($"user-{user.ID}", count);
        }

        /// <summary>
        /// Get events for the given key.
        /// </summary>
        /// <param name="key">The stream key.</param>
        /// <param name="count">The number of events to retrieve.</param>
        /// <returns>Awaitable task for the events.</returns>
        public async Task<IEnumerable<Models.RecordedEvent>> GetEvents(string key, int count = 100)
        {
            var events = await eventStoreConnection.ReadStreamEventsBackwardAsync(key, StreamPosition.End, count, true);

            return events.Events.Select(e =>
            {
                var json = Encoding.UTF8.GetString(e.Event.Data);

                try
                {
                    var eventData = JsonConvert.DeserializeObject<Models.RecordedEvent>(json);

                    eventData.Created = e.Event.Created;

                    return eventData;
                }
                catch (Exception)
                {
                    // Bad JSON Object, return null for now.
                    return null;
                }
            }).Where(e => e != null);
        }

        /// <summary>
        /// Remove events for an user.
        /// </summary>
        /// <param name="user">The user to remove the events.</param>
        /// <returns>Awaitable task.</returns>
        public Task RemoveEvents(Models.User user)
        {
            return RemoveEvents($"user-{user.ID}");
        }

        /// <summary>
        /// Remove events on key.
        /// </summary>
        /// <param name="key">The stream key to remove.</param>
        /// <returns>Awaitable task.</returns>
        public async Task RemoveEvents(string key)
        {
            await eventStoreConnection.DeleteStreamAsync(key, ExpectedVersion.Any);
        }
    }
}
