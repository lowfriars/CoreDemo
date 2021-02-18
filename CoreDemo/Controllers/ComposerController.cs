using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CoreDemo.Logging;
using Microsoft.EntityFrameworkCore;
using CoreDemo.Data;
using Microsoft.AspNetCore.Identity;
using CoreDemo.Models;
using System;

namespace CoreDemo.Controllers
{
    /// <summary>
    /// This is a sample of using .Net Core to create a simple master/detail database. It's intended to illustrate 
    /// the basic use of ASP.Net MVC, Entity Framework, Identity and Authentication and custom Event Logging.
    /// 
    /// This class implements the MVC "Controller" for a simple database of Composers, recording their names, dates of birth and death
    /// and their works (as detail records in a separate tabled linked by a foreign key).
    /// </summary>
    [Authorize(Policy = "DatabaseOnly")]                                        // Allow access only to users in the Database role
    public class ComposerController : Controller
    {
                                                                                // Saved injected dependencies
        private readonly CoreDemoDbContext _context;                            // The database context used by EF
        private readonly IOptions<AppSettings> _settings;                       // Configuration options
        private readonly ILogger<ComposerController> _logger;                   // A logging instance
        private readonly UserManager<ApplicationUser> _userManager;             // A user-manager instance


    /// <summary>
    /// Controller constructor - saves the injected dependencies we need later
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="settings">Configuration settings</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="userManager">User manager instance</param>
        public ComposerController
            (
            CoreDemoDbContext context,
            IOptions<AppSettings> settings,
            ILogger<ComposerController> logger,
            UserManager<ApplicationUser> userManager
            )
        {
            _context = context;
            _settings = settings;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Index action
        /// </summary>
        /// <returns>List of the current composers as a view (ordered by Name)</returns>
        public IActionResult Index()
        {
            return View(_context.Composers.ToList().OrderBy( x => x.Name));
        }

        /// <summary>
        /// Request to initiate a change of composer information.
        /// </summary>
        /// <param name="id">Record identity in database</param>
        /// <returns>View containing selected record (or redirects to home page to display error)</returns>
        [HttpGet]
        public IActionResult Change(int id)
        {
            Composers c;
            try
            {
                if (!_context.Composers.Any(p => p.Id == id))                   // Check the record exists
                    return RedirectToAction(nameof(ComposerController.Index));

                c = _context.Composers.Where(p => p.Id == id).First();          // There should only be one, but...
                return View(c);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });

            }
        }

        /// <summary>
        /// Details of the record are fetched with a GET operation. The amended details are
        /// sent by a POST.
        /// </summary>
        /// <param name="c">The model returned by the POST operation</param>
        /// <returns>Redirects to the composer detail page if successful, or to home to display error message</returns>
        [HttpPost, ActionName("Change")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeConfirmed(Composers c)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Composers.Update(c);                   // Update the database
                    _context.SaveChanges();                         // ... saving changes
                    _logger.LogError(EvtCodes.evtComposerChangeOk, "Changed composer {c}", c.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(EvtCodes.evtComposerChangeFail, ex, "Failed to change composer {c}", c.Name);
                    return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });
                }
            }

            return RedirectToAction(nameof(ComposerController.Detail), new { id = c.Id });
        }

        /// <summary>
        /// The first phase of creating a new record is to return an empty model from
        /// which the view will be created
        /// </summary>
        /// <returns>Unpopulated view</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// The completed model is returned in a POST operation when the user fills in the form
        /// </summary>
        /// <param name="composer">The completed model</param>
        /// <returns>Redirects to composer list on success, to home in the case of a serious error, or the model with error information updated</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Composers composer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Composers.Add(composer);
                    _context.SaveChanges();
                    _logger.LogError(EvtCodes.evtComposerDeleteOk, "Created composer {c}", composer.Name);
                    return RedirectToAction(nameof(ComposerController.Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(EvtCodes.evtComposerAddFail, ex, "Failed to add composer {c}", composer.Name);
                    return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });
                }
            }

            return View(composer);
        }


        /// <summary>
        /// Returns detail for a specific composer. The composer's works are explicitly added
        /// as we use a partial view to display these "detail" records below the information about
        /// the composer.
        /// </summary>
        /// <param name="id">The identity of the composer database record</param>
        /// <returns>View containing composer and work details (or redirects to composer list if not found)</returns>
        [HttpGet]
        public IActionResult Detail(int id)
        {
            Composers c;

            if (!_context.Composers.Any(p => p.Id == id))
                return RedirectToAction(nameof(ComposerController.Index));

            c = _context.Composers.Include(p => p.Works).Where(p => p.Id == id).First();

            return View(c);
        }

        /// <summary>
        /// Like creation, deletion is in two parts - the current value is retrieved and shown to the 
        /// user and then a POST operation is used to confirm the deletion should proceed
        /// 
        /// We also calculate the number of works belonging to the composer and issue a warning if it's
        /// non-zero as these will also be deleted (cascaded deletion). If we don't explicitly query the
        /// number of works, it will typically appear to be zero because of lazy-loading.
        /// 
        /// </summary>
        /// <param name="id">The identity of the composer database record</param>
        /// <returns>View containing current data, or redirects to composer list if not found.</returns>
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Composers c;

            if (!_context.Composers.Any(p => p.Id == id))
                return RedirectToAction(nameof(ComposerController.Index));

            c = _context.Composers.Where(p => p.Id == id).First();

            ViewBag.WorkCount = _context.Entry(c).Collection(w => w.Works).Query().Count();

            return View(c);
        }

        /// <summary>
        /// Actual composer record deletion
        /// </summary>
        /// <param name="id">The database record identity for the composer record</param>
        /// <param name="composerName">The composer name (in case the record has already been deleted and we can't find it)</param>
        /// <returns>Redirects to the list of composers</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, string composerName)
        {
            Composers c;

            if (!_context.Composers.Any(p => p.Id == id))
                return RedirectToAction(nameof(ComposerController.Index));

            try
            {
                c = _context.Composers.Where(p => p.Id == id).Single();
                _context.Composers.Remove(c);
                _context.SaveChanges();
                _logger.LogError(EvtCodes.evtComposerDeleteOk, "Deleted composer {c}", c.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(EvtCodes.evtComposerDeleteFail, ex, "Failed to delete composer {c}", composerName);
                return RedirectToAction(nameof(HomeController.Index), "Home", new { error = ex.Message });
            }

            return RedirectToAction(nameof(ComposerController.Index));
        }
    }
}
