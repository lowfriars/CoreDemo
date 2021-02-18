using CoreDemo.Logging;
using CoreDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreDemo.Controllers
{
    /// <summary>
    /// This is the controller for User data.
    /// Unlike the other controllers, it uses the UserManager to store and retrieve UserData - the UserManager
    /// uses Entity Framework behind the scenes and is relentlessly asynchronous.
    /// </summary>
    [Authorize(Policy = "AdministratorOnly")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;     // UserManager instance saved from constructor
        private readonly ILogger<AccountController> _logger;            // Logger instantce saved from constructor

        /// <summary>
        /// Constructor - save the service instances we need that are made available via Dependence Injection
        /// </summary>
        /// <param name="userManager">UserManager instance</param>
        /// <param name="logger">Logger instance</param>
        public UserController
                (
                UserManager<ApplicationUser> userManager,
                ILogger<AccountController> logger
                )
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// GET User/Index
        /// Retrieves the users and their roles and returns a corresponding view (asynchronously)
        /// </summary>
        /// <returns>User view</returns>
        public async Task<IActionResult> Index()
        {
            List<Models.ApplicationUser> users = _userManager.Users.OrderBy(u => u.UserName).ToList();
            foreach (Models.ApplicationUser u in users)
            {
                u.RoleNames = await _userManager.GetRolesAsync(u) as List<string>;
            }

            return View(users);
        }

        /// <summary>
        /// Initial part of user creation - returns an empty model view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Second part of user creation - the view model is POSTed back
        /// 
        /// This is a very basic implementation - each user can only be in one role
        /// </summary>
        /// <param name="model">The model posted back by the administrative user</param>
        /// <returns>Redirects to "/User/Index" is successful, to "Home" if an error is thrown or represents the View with errors added to the Model</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.UserViewModels.CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Models.ApplicationUser u = await _userManager.FindByNameAsync(model.UserName);          // Check is user already exists
                    if (u == null) 
                    {
                        u = new ApplicationUser
                        {
                            UserName = model.UserName
                        };
                        var r = await _userManager.CreateAsync(u, model.NewPassword);                       // Attempt to create user

                        if (r.Succeeded)
                        {
                            r = await _userManager.AddToRoleAsync(u, model.UserRole);                       // Add user to chosen role
                            if (r.Succeeded)
                            {
                                _logger.LogInformation(EvtCodes.evtUserAddOk, "Created new user {U}", model.UserName);
                                return RedirectToAction("Index");
                            }
                        }

                        AddErrors(r);
                        _logger.LogWarning(EvtCodes.evtUserAddFail, "Failed to create user {U} (m)", model.UserName, r.Errors.FirstOrDefault().Description);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User already exists");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    _logger.LogError(EvtCodes.evtUserAddFail, ex, "Exception occured creating user {U}", model.UserName);
                }
            }
            return View(model);
        }
        /// <summary>
        /// First part of user deletion - return a view based on the selected user and await confirmation
        /// </summary>
        /// <param name="id">UserManager GUID of user</param>
        /// <returns>User view, or redirects to Index page if not found</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(string id = null)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                user.RoleNames = new List<string>();

                if (user == null)
                    return RedirectToAction("Index");

                user.RoleNames = await _userManager.GetRolesAsync(user) as List<string>;
                return View(user);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }

        /// <summary>
        /// POST operation to confirm user deletion.
        /// 
        /// Remove the user from UserManager
        /// </summary>
        /// <param name="id">User GUID</param>
        /// <returns>Redirects to /User/Index if successful or to /Home/Index to in event of exception</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id = null)
        {
            if (id == null)
                return RedirectToAction("Index");

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return RedirectToAction("Index");

                var r = await _userManager.DeleteAsync(user);
                if (r.Succeeded)
                {
                    _logger.LogInformation(EvtCodes.evtUserDeleteOk, "Deleted user {U}", user.UserName);
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogError(EvtCodes.evtUserDeleteFail, "Failed to deleted user {U} (m)", user.UserName, r.Errors.FirstOrDefault().Description);
                    return RedirectToAction("Index", "Home", new { error = GetErrors(r) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(EvtCodes.evtUserDeleteFail, ex, "Exception occured deleting user {U}", id);
                return RedirectToAction("Index", "Home", new { error = ex.Message });
            }

        }

        /// <summary>
        /// Utility routine to add errors from the UserManager to the Model View
        /// </summary>
        /// <param name="result">Result returned from UserManager</param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        /// <summary>
        /// Utility routine to add errors from the UserManager to a string
        /// </summary>
        /// <param name="result">UserManager result</param>
        /// <returns>String with errors concatentated</returns>
        private static string GetErrors(IdentityResult result)
        {
            string s = "";

            foreach (var error in result.Errors)
            {
                if (s != "")
                    s += "; ";
                s += error.Description;
            }

            return s;
        }

    }
}
