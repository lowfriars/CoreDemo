using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreDemo
{
    /// <summary>
    /// Class describing the application settings (values found appsettings.json)
    /// </summary>
    public class AppSettings
    {
        public string AccessDeniedPage { get; set; }            // Redirection URL when access is denied
        public string AdminPolicy { get; set; }                 // Name of the "Administrators Only" policy
        public string AdminRole { get; set; }                   // Name of the Administrator role
        public string ApplicationTitle { get; set; }            
        public string ApplicationCopyright { get; set; }
	    public string DatabaseConnectionMSSQL { get; set; }     // Database connection string
        public string DatabaseConnectionSqlite { get; set; }
        public string DatabasePolicy { get; set; }              // Name of policy for database editing
        public string DatabaseRole { get; set; }                // Name of the role for database editing
        public string InitialUser { get; set; }                 // Initial username and password
        public string InitialPassword { get; set; }
		public int LoginTimeout { get; set; }
        public string DefaultErrorPage { get; set; }            // Redirection URL for errors
        public string LoginPage { get; set; }                   // Redirection URL to request Login
        public bool PasswordDigitRequired { get; set; }         // Password complexity
        public bool PasswordLowercaseRequired { get; set; }
        public bool PasswordUppercaseRequired { get; set; }
        public bool PasswordSymbolRequired { get; set; }
        public bool PasswordLockoutEnabled { get; set; }
        public int PasswordMinLength { get; set; }
        public int PasswordMaxFailures { get; set; }
        public int PasswordFailureLockoutHours { get; set; }
        public int PasswordFailureLockoutMins { get; set; }
        public int PasswordMaxLifetimeDays { get; set; }
        public string PasswordValidityClaim { get; set; }       // Name of claim used to assert password validity / flag expiry
    }
}
