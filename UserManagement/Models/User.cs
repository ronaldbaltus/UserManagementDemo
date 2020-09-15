namespace UserManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Data representation of an user.
    /// </summary>
    public class User
    {
        [NotMapped]
        private string emailAddressValue = string.Empty;

        /// <summary>
        /// Gets the events that happened during the runtime existence of this user.
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
        public int ID
        {
            get;
            set;
        }

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
                if (emailAddressValue != value)
                {
                    emailAddressValue = value;
                    EmailAddressVerified = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        [Required]
        [MinLength(6)]
        [MaxLength(255)]
        [NotMapped]
        public string Password
        {
            get => "******";
            set
            {
                // Quickly hash the password.
                HashedPassword = Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
        }

        /// <summary>
        /// Gets or sets the hashed value of the password.
        /// </summary>
        public string HashedPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the e-mail address is verified.
        /// </summary>
        public bool EmailAddressVerified
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the date and time when this user was scheduled for deletion.
        /// </summary>
        public DateTime? RemovedAt
        {
            get;
            set;
        }
    }
}
