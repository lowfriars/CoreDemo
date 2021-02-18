using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CoreDemo.Logging
{ 
    /// <summary>
    /// The data model for the event logger
    /// </summary>
    public partial class DatabaseLog
    {
        public const int MaximumExceptionLength = 2000;

        public int Id { get; set; }                         // Database identity column
        public DateTime Date { get; set; }                  // Date and time of event
        [Required]
        public int EventId { get; set; }                    // Our locally-chosen event id
        [Required]
        public int Level { get; set; }                      // Event level
        [Required]
        [StringLength(255)]
        public string Logger { get; set; }                  // Logger name
        [Required]
        public string Message { get; set; }                 // Event message

        [StringLength(MaximumExceptionLength)]
        public String Exception { get; set; }               // Exception text
        [StringLength(50)]
        public string Username { get; set; }                // Logged-in user
        [StringLength(50)]
        public string TargetUsername { get; set; }          // User who is target of operation
        [StringLength(100)]
        public string Url { get; set; }                     // URL at which event occurred
    }
}
