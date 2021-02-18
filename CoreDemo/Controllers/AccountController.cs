using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CoreDemo.Models.AccountViewModels;
using CoreDemo.Logging;
using CoreDemo.Models;

namespace CoreDemo.Controllers
{
    /// <summary>
    /// Controller for account-related actions - Login, LogOff, ChangePassword.
    ///
    /// Also contains the default "Forbidden" action when access is denied.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AppSettings _settings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController
                (
                IOptions<AppSettings> settings,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
                ILogger<AccountController> logger
                )
        {
            _settings = settings.Value;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// When invoked with GET, display login form
        /// </summary>
        /// <param name="returnUrl">Optional URL to return to when action succeeds</param>
        /// <returns>Model View</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        //
        // POST: /Account/Login
        /// <summary>
        /// When involed with POST, validate username and password
        /// </summary>
        /// <param name="model">View Model from POST</param>
        /// <param name="returnUrl">Optional URL to return to if operation succeeds</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login (LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if (model.UserName.Length > 0)
                {
                    try
                    {
                        var user = await _userManager.FindByNameAsync(model.UserName);
                        var loginClaim = new Claim(_settings.PasswordValidityClaim, "Yes");
                        if (user == null)
                        {
                            _logger.LogWarning(EvtCodes.evtLogInFail, "Invalid user name {U}", model.UserName);
                            ModelState.AddModelError("UserName", "No such user");
                        }
                        else
                        {
                            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: _settings.PasswordLockoutEnabled);
                            if (result.Succeeded)
                            {
                                if (DateTime.Now - user.LastPasswordChange > new TimeSpan(_settings.PasswordMaxLifetimeDays, 0, 0, 0))
                                {
                                    await _userManager.RemoveClaimAsync(user, loginClaim);
                                    return RedirectToAction("ChangePassword", new { returnUrl, forced = "yes" });
                                }
                                user.LastLogin = DateTime.Now;
                                await _userManager.UpdateAsync(user);

                                _logger.LogInformation(EvtCodes.evtLogInOk, "Logged in user {U}", model.UserName);
                                return RedirectToLocal(returnUrl);
                            }
                            else
                            {
                                user.LoginFailures += 1;
                                await _userManager.UpdateAsync(user);
                            }

                            if (result.IsLockedOut)
                            {
                                _logger.LogError(EvtCodes.evtLogInFail, "Account locked out for user {U}", model.UserName);
                                ModelState.AddModelError("UserName", "Account Locked Out");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }

            return View(model);
        }

        /// <summary>
        /// Useful utility to redirect to a specific URL, esnuring it's local
        /// </summary>
        /// <param name="returnUrl">The desired target URL</param>
        /// <returns>The result of a redirection action either to the given URL or to "/" if it is not local</returns>
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// The target of an explicit logoff action request (POST to "logoff" action on "home" controller)
        /// 
        /// Causes the user to be signed out and a redirection to the index page.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            string user = HttpContext.User.Identity.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation(EvtCodes.evtLogOut, "Logged out user {U}", user);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        //
        /// <summary>
        /// Initial part of a Change Password request.
        /// </summary>
        /// <param name="returnUrl">URL to return to (optional)</param>
        /// <param name="forced">String to indicate if password change is required by system rather than voluntary</param>
        /// <returns>A view representing the contents of the password-change form</returns>
        [HttpGet]
        public IActionResult ChangePassword(string returnUrl = null, string forced = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Forced"] = forced;
            return View();
        }

        /// <summary>
        /// Password change action. Requests the usermanager to change the user's password
        /// </summary>
        /// <param name="model">Data returned from change-password form</param>
        /// <param name="returnUrl">Optional URL to returnto </param>
        /// <param name="forced">String to indicate if password change was forced by system</param>
        /// <returns>Redirects to given return URL or re-presents form in case of a password error</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, string returnUrl = null, string forced = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Forced"] = forced;
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user != null)
                    {
                        if (model.NewPassword != model.OldPassword)
                        {
                            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                            if (result.Succeeded)
                            {
                                var loginClaim = new Claim(_settings.PasswordValidityClaim, "Yes");
                                user.LastPasswordChange = DateTime.Now;
                                await _userManager.UpdateAsync(user);
                                await _userManager.AddClaimAsync(user, loginClaim);
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                _logger.LogInformation(EvtCodes.evtPwdChangeOK, "Changed password for user {U}", HttpContext.User.Identity.Name);

                                return RedirectToLocal(returnUrl);
                            }
                            AddErrors(result);
                            _logger.LogWarning(EvtCodes.evtPwdChangeFail, "Failed to change password for user {U} (m)", HttpContext.User.Identity.Name, result.Errors.FirstOrDefault().Description);
                        }
                        else
                        {
                            ModelState.AddModelError("NewPassword", "New password must be different from old");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(EvtCodes.evtPwdChangeFail, ex, "Exception changing password for user {U}", HttpContext.User.Identity.Name);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Utility routine to add errors from the user manager to the View Model State where they will be presented to the user
        /// </summary>
        /// <param name="result"></param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        /// <summary>
        /// The action to which users are redirected when they are trying to invoke an action that their role does not permit.
        /// This is only triggered by the .Net authorisation and authentication components - the path to the "AccessDeniedPath" is
        /// configured in Startup.cs.
        /// </summary>
        /// <param name="returnUrl">The URL at which access was denied.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Forbidden(string returnUrl = null)
        {
            bool hasLoginClaim = false;
            var loginClaim = new Claim(_settings.PasswordValidityClaim, "Yes");

            if (_signInManager.IsSignedIn(HttpContext.User))
            {
                foreach (Claim c in HttpContext.User.Claims)
                {
                    if (c.Type == loginClaim.Type && c.Value == loginClaim.Value)
                        hasLoginClaim = true;
                }
                if (!hasLoginClaim)
                {
                    _logger.LogWarning(EvtCodes.evtPwdChangeDemand, "Password change required for user {U}", HttpContext.User.Identity.Name);
                    return RedirectToAction("ChangePassword", new { returnUrl, forced = "yes" });
                }
            }

            if (_signInManager.IsSignedIn(HttpContext.User))
                _logger.LogError(EvtCodes.evtForbidden, "Attempt to access forbidden page {p} by user {U}", returnUrl, HttpContext.User.Identity.Name);
            else
                _logger.LogError(EvtCodes.evtForbidden, "Attempt to access forbidden page {p} by unknown user", returnUrl);

            return View();
        }
    }
}
