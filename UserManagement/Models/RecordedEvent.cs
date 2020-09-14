namespace UserManagement.Models
{
    using System;

    /// <summary>
    /// Representation of an event coming from the database.
    /// </summary>
    public class RecordedEvent : Event
    {
        /// <summary>
        /// Gets or sets when the event was created.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
