using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace UserManagement.Models
{
    /// <summary>
    /// Data representation of an user.
    /// </summary>
    public class User
    {
        private string emailAddressValue = null;
        private string passwordValue = null;

        /// <summary>
        /// Events that happened during the runtime existence of this user.
        /// </summary>
#if !DEBUG
        [JsonIgnore]
#endif
        [NotMapped]
        public List<Event> RuntimeEvents { get; private set; } = new List<Event>();

        /// <summary>
        /// Gets or sets the unique ID to identify this user.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the e-mail address of the user.
        /// </summary>
        [Required]
        [MinLength(6)]
        [MaxLength(255)]
        [RegularExpression(@"^[\w\.\-_]+@[\w\.\-_]+\.[\w\.\-_]{2,5}$", ErrorMessage = "E-mail address is not in a valid format")]
        public string EmailAddress
        {
            get => emailAddressValue;
            set
            {
                //RuntimeEvents.Add(new Event()
                //{
                //    Type = emailAddressValue == null ? Event.EventType.Create : Event.EventType.Update,
                //    Fieldname = nameof(EmailAddress),
                //    PreviousValue = emailAddressValue,
                //    NewValue = value,
                //});
                emailAddressValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        [Required]
        [MinLength(6)]
        [MaxLength(255)]
        public string Password
        {
            get => passwordValue;
            set
            {
                // Quickly hash the password.
                value = Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value)));

                //RuntimeEvents.Add(new Event()
                //{
                //    Type = passwordValue == null ? Event.EventType.Create : Event.EventType.Update,
                //    Fieldname = nameof(Password),
                //    PreviousValue = passwordValue,
                //    NewValue = value,
                //});
                passwordValue = value;
            }
        }
    }
}
