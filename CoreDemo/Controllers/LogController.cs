using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoreDemo.Data;

namespace CoreDemo.Controllers
{
    /// <summary>
    /// The controller for viewing log entries. Only "Index" is supported as an action and it returns
    /// a View containing the events ordered with the most recent first.
    /// </summary>
    [Authorize(Policy = "AdministratorOnly")]
    public class LogController : Controller
    {
        private readonly CoreDemoEvtContext _context;           // Database context saved from constructor

        /// <summary>
        /// Constructor - save the database context provided by dependency injection for later use.
        /// </summary>
        /// <param name="context">Database context</param>
        public LogController
            (
            Data.CoreDemoEvtContext context
            )
        {
            _context = context;
        }

        /// <summary>
        /// Return a view based on the event log records returned from the database ordered by most recent first
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View(_context.Logs.OrderByDescending (x => x.Date).ToList());
        }


    }
}
