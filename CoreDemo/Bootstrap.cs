using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using CoreDemo.Models;

namespace CoreDemo
{
    /// <summary>
    /// This is a simple helper to seed the Identity database with an initial user and the user roles
    /// </summary>

    public class Bootstrap
    {
        private readonly AppSettings _settings;
        private  string _adminRole, _initialUser, _initialPassword;
        private  List<string> _additionalRoles;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public Bootstrap(IOptions<AppSettings> settings, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _settings = settings.Value;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void SeedUsersAndRoles(string adminRole, List<string> additionalRoles, string initialUser, string initialPassword)
        {
            _adminRole = adminRole;
            _initialPassword = initialPassword;
            _initialUser = initialUser;
            _additionalRoles = additionalRoles;

            // We use this third-party helper as this function needs to run synchronously and the user/role manager is asynchronous.

            AsyncContext.Run (SeedUsersAndRolesAsync);
        }
        public async Task<bool> SeedUsersAndRolesAsync ()
        {

            var user = new Models.ApplicationUser
            {
                UserName = _initialUser,
                NormalizedUserName = _initialUser,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            _additionalRoles.Add(_adminRole);

            foreach (string s in _additionalRoles)
            {
                if (await _roleManager.FindByNameAsync(s) == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = s });
                }
            }

            if (await _userManager.FindByNameAsync(user.UserName) == null)
            {
                user.LockoutEnabled = _settings.PasswordLockoutEnabled;
                user.EmailConfirmed = true;
                await _userManager.CreateAsync(user, _initialPassword);
                await _userManager.AddToRoleAsync(user, _adminRole);
            }

            return true;
        }

    }
}
