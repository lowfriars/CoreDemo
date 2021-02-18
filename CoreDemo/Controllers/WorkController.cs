using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CoreDemo.Logging;
using System.Text.RegularExpressions;
using CoreDemo.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreDemo.Controllers
{
    /// This is a sample of using .Net Core to create a simple master/detail database. It's intended to illustrate 
    /// the basic use of ASP.Net MVC, Entity Framework, Identity and Authentication and custom Event Logging.
    /// 
    /// This class implements the MVC "Controller" for a simple database of the works of a Composer, consisting of
    /// a title, year of composition and description - and a foreign key reference to the composer.

    [Authorize(Policy = "DatabaseOnly")]                            // Allow access only to users in the Database role
    public class WorkController : Controller
    {
        private readonly CoreDemoDbContext _context;                // Instance of database context saved from constructor
        private readonly ILogger<WorkController> _logger;           // Logging instance saved from constructor

        /// <summary>
        /// Constructor - save references to the services provided by dependency injection
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logger">Logging instance</param>
        public WorkController
            (
            CoreDemoDbContext context,
            ILogger<WorkController> logger
            )
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// First part of a delete operation - return a view containing the data about the work and await confirmation of deletion
        /// </summary>
        /// <param name="composerName">Composer name (as we don't have it in the work record)</param>
        /// <param name="id">Database identity column value</param>
        /// <returns>View containing the model data (or a redirection to /Composer/Index in the event of not finding it or to Home if exception)</returns>
        public ActionResult Delete(string composerName, int id)
        {
            ViewBag.ComposerName = composerName;

            if (!_context.Works.Any(p => p.Id == id))                                   // Does this work still exist?
                return RedirectToAction(nameof(ComposerController.Index), "Composer");

            try
            {
                Works work = _context.Works.Where(c => c.Id == id).Single();
                return View(work);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST action to confirm deletion - attempt to delete
        /// </summary>
        /// <param name="w">Model returned by POST operation</param>
        /// <param name="composerName">Composer name (as we don't have it in record and we need it for logging)</param>
        /// <returns>Redirects to composer detail if successful or to home if not</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Works w, string composerName)
        {
            try
            {
                Works work = _context.Works.Where(c => c.Id == w.Id).Single();
                _context.Works.Remove(work);
                _context.SaveChanges();
                _logger.LogError(EvtCodes.evtWorkDeleteOk, "Deleted work {w} by composer {c}", work.Title, composerName);
                return RedirectToAction(nameof(ComposerController.Detail), "Composer" , new { id = work.ComposerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(EvtCodes.evtWorkDeleteFail, ex, "Failed to delete work by composer {c}", composerName);
                return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });
            }
        }

        /// <summary>
        /// First part of changing information about a work - retrieve the details and return a Model View accordingly
        /// </summary>
        /// <param name="id">Value of database identity column</param>
        /// <param name="composerName">Name of composer (it's not in the record)</param>
        /// <param name="composerId">Value of composer identity column for work's composer</param>
        /// <returns>Redirects to composer detail if work not found, to home if an exception occurs or returns View of work</returns>
        [HttpGet]
        public IActionResult Change(int id, string composerName, int composerId)
        {
            Works w;
            try
            {
                if (!_context.Works.Any(p => p.Id == id))       // Check the work still exists
                    return RedirectToAction(nameof(ComposerController.Detail), "Composer", new { id = composerId });

                w = _context.Works.Where(p => p.Id == id).First();

                ViewBag.ComposerName = composerName;
                return View(w);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });

            }
        }

        /// <summary>
        /// Receive POST to confirm changing record
        /// </summary>
        /// <param name="w">Update model posted back by user</param>
        /// <param name="composerName">Composer name (for logging)</param>
        /// <returns>Redirects to composer detail page, or to Home page if exception occurs</returns>
        [HttpPost, ActionName("Change")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeConfirmed(Works w, string composerName)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Works.Update(w);
                    _context.SaveChanges();
                    _logger.LogError(EvtCodes.evtWorkChangeOk, "Changed work {w} for composer {c}", w.Title, composerName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(EvtCodes.evtWorkChangeFail, ex, "Failed to change work {w} for composer {c}", w.Title, composerName);
                    return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });
                }
            }

            return RedirectToAction(nameof(ComposerController.Detail), "Composer", new { id = w.ComposerId });
        }

        /// <summary>
        /// First part of creation for new work - return an empty view
        /// </summary>
        /// <param name="composerId">Identity value of work's composer</param>
        /// <param name="composerName">Name of work's composer</param>
        /// <returns></returns>
        public IActionResult Create(int composerId, string composerName)
        {
            ViewBag.ComposerId = composerId;            // Save these in the ViewBag to avoid having to look them up later
            ViewBag.ComposerName = composerName;
            return View();
        }

        /// <summary>
        /// Final part of new work creation - receive POSTed updated model and attempt to save it to the database.
        /// </summary>
        /// <param name="composerId">Identity value of composer record</param>
        /// <param name="composerName">Name of composer</param>
        /// <param name="work">Model containing updated work details</param>
        /// <returns>Redirects to composer detail if successful or to home if there's an exception</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int composerId, string composerName, Works work)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    work.ComposerId = composerId;
                    _context.Works.Add(work);
                    _context.SaveChanges();
                    _logger.LogError(EvtCodes.evtWorkAddOk, "Added work {w} by composer {c}", work.Title, composerName);
                    return RedirectToAction("Detail", "Composer", new { id = composerId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(EvtCodes.evtWorkAddFail, ex, "Failed to add work by composer {c}", composerName);
                    return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });
                }
            }

            return View(work);
        }
    }
}
