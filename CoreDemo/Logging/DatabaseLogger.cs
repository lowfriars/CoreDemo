using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Concurrent;

namespace CoreDemo.Logging
{
    public class DatabaseLoggerOptions
    {
        public Dictionary<string, LogLevel> Filters { get; set; }
    }

    /// <summary>
    /// Basic outline of event logging to a file using Entity Framework for simplicity (probably too heavyweight for production use)
    /// 
    /// There are two main parts, the Logger Provider and the Logger: the Logger Provider creates instances of the Logger.
    /// 
    /// There may be a lot of instances as each service may create its own.
    /// 
    /// We keep a collection of logger instances and dispose of them when the LoggerProvider is Disposed.
    /// </summary>
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;             // Local reference to application service provider
        private readonly IHttpContextAccessor _httpContextAccessor;     // Local reference to HTTP context
        private bool isDisposed;                                        // For IDispose
        private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers = new ConcurrentDictionary<string, DatabaseLogger>(); // List of Logger instances

        /// <summary>
        /// Constructor: save necessary context for later
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="filter">Filter for this instance of logging</param>
        public DatabaseLoggerProvider(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;    
        }

        /// <summary>
        /// Return a new Logger, saving a local reference for later disposal
        /// </summary>
        /// <param name="name">Logger Name</param>
        /// <returns>Logger Instance</returns>
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, name => new DatabaseLogger(name, _httpContextAccessor, _serviceProvider));
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        public void Dispose() 
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            if (isDisposing)
            {
                _loggers.Clear();
            }

            isDisposed = true;
        }
    }

    /// <summary>
    /// Individual logger instances
    /// </summary>
    public class DatabaseLogger : ILogger 
    {
        private readonly string _name;                                      // Logger name
        private readonly IHttpContextAccessor _httpContextAccessor;         // HTTP context accessor
        private readonly IServiceProvider _services;                        // Application service provider

        /// <summary>
        /// Constructor - save information for later use
        /// </summary>
        /// <param name="name">Logger name</param>
        /// <param name="httpContextAccessor">HTTP Context accessor</param>
        /// <param name="serviceProvider">Application service provider</param>
        public DatabaseLogger(string name, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _name = name;
            _httpContextAccessor = httpContextAccessor;
            _services = serviceProvider;
        }

        /// <summary>
        /// When you log an event, it's associated with a level (such as WARN), a numeric Id, an optional Exception and state information -
        /// the event message which may contain substitution parameters and the values of those parameters. A formatting function is available
        /// to turn the "state" into a text string.
        /// 
        /// More information here:
        ///     https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
        /// </summary>
        /// <typeparam name="TState">Template type for logging state information</typeparam>
        /// <param name="logLevel">Logging level</param>
        /// <param name="eventId">Event id</param>
        /// <param name="state">Logging state - collection of subsitution keys and their associated values from the original logging request</param>
        /// <param name="exception">Any exception information</param>
        /// <param name="formatter">Formatter to create text from event state</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                string targetUser = "";

                if (!IsEnabled(logLevel))
                {
                    return;
                }

                // We interecept a specific subsitution paramter and log its value in a separate database column too.
                // A message that includes {U} specifies a target user that is affected by an operation by the present user

                var tstate = state as IEnumerable;
                foreach (KeyValuePair<string, object> t in tstate)
                {
                    if (t.Key == "U")
                        targetUser = t.Value.ToString();
                }

                var message = string.Empty;
                if (formatter != null)
                {
                    message = formatter(state, exception);
                }
                else
                {
                    message = "(No formatter for message)";
                }

                var log = new DatabaseLog
                {
                    Message = message, 
                    Date = DateTime.UtcNow,
                    Level = (int)logLevel,
                    Logger = _name,
                    EventId = eventId.Id                    
                };

                if (exception != null)
                    log.Exception = Trim(exception.ToString(), DatabaseLog.MaximumExceptionLength);

                // If we have access to HTTP context, we also get the logged-in username and log that too
                if (_httpContextAccessor.HttpContext is HttpContext context)
                {
                    log.Username = context.User.Identity.Name;
                    log.Url = context.Request.Path;
                    if (targetUser == "")
                        targetUser = context.User.Identity.Name;
                }

                log.TargetUsername = targetUser;

                // We need to create a scope to get the right scope for the database context 
                var serviceScopeFactory = _services.GetService<IServiceScopeFactory>();
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    // Use Entity Framework to add the record to the database
                    var db = scope.ServiceProvider.GetRequiredService<Data.CoreDemoEvtContext>();
                    db.Set<DatabaseLog>().Add(log);

                    db.SaveChanges();
                }
            }
        catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        private static string Trim(string value, int maximumLength)
        {
            return value.Length > maximumLength ? value.Substring(0, maximumLength) : value;
        }

        /// <summary>
        /// This function is called to check if logging is enabled for a specifc level of
        /// event.
        /// 
        /// When you add an event logger, it can be a sink for all sorts of potential events.
        /// We only want to log events related to this application and we want to all of them.
        /// 
        /// So we return "true" if the name of this logger instance begins with "CoreDemo".
        /// 
        /// A more sophisticated logger could implement more complicated filters based on the 
        /// name of the event source and the severity of the event.
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <returns>True if logging enabled for this event</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _name.StartsWith("CoreDemo");
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }

}

