using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreDemo.Models
{
    /// <summary>
    /// This is a model for recording the data associated with users - extends the Identity Model provided by .Net
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public enum UserStatusType { Pending = 0, Active, Historic, Suspended }
        public int LoginFailures { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public UserStatusType Status { get; set; }              // Not currently used 
        [NotMapped]
        public List<string> RoleNames { get; set; }
    }
}
