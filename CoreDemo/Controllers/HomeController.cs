using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CoreDemo.Controllers
{
    /// <summary>
    /// This is the Home controller - its only role is to return a home-page view
    /// which will normally be unpopulated, but in the case that the page has been 
    /// called with an error in the query string, the error message will be added to the model for display.
    /// </summary>
    [AllowAnonymous]
    public class HomeController : Controller
    {     
        public HomeController()
        {
        }

        public IActionResult Index(string error = "")
        {
            ViewData["Error"] = error;
            return View();
        }

              
    }
}
