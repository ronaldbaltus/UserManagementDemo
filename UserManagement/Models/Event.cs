namespace UserManagement.Models
{
    /// <summary>
    /// An immutable historic record of what has changed to a resource.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Type of event.
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// Event that created a resource.
            /// </summary>
            Create,

            /// <summary>
            /// Event that updated a resource.
            /// </summary>
            Update,

            /// <summary>
            /// Event that deleted a resource.
            /// </summary>
            Delete,
        }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        public EventType Type { get; set; }

        /// <summary>
        /// Gets or sets the field name that was affected.
        /// </summary>
        public string Fieldname { get; set; }

        /// <summary>
        /// Gets or sets the previous value.
        /// </summary>
        public string PreviousValue { get; set; }

        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        public string NewValue { get; set; }
    }
}
