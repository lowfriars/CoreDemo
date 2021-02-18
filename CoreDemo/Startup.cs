using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using CoreDemo.Data;
using CoreDemo.Logging;
using CoreDemo.Models;

namespace CoreDemo
{
    public class Startup
    {
        protected bool isDev;
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                isDev = true;
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var pwOptions = new PasswordOptions();
            var lockoutOptions = new LockoutOptions();

            // Add framework services.
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));           // Get configuration information
            services.AddOptions();

            // Set up database type, connection string and contexts
            // Define "MSSQL" to use SQLServer instead (connection strings can be found in appsettings.json)

#if (MSSQL)
            var connection = Configuration.GetSection("AppSettings").GetValue<string>("DatabaseConnectionMSSQL");

            services.AddDbContext<CoreDemoDbContext>(options => 
                options.UseSqlServer(connection));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connection));
            services.AddDbContext<CoreDemoEvtContext>(options =>
                options.UseSqlServer(connection));
              
#else
            var connection = Configuration.GetSection("AppSettings").GetValue<string>("DatabaseConnectionSqlite");
            services.AddDbContext<CoreDemoDbContext>(options =>
                options.UseSqlite(connection));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connection));
            services.AddDbContext<CoreDemoEvtContext>(options =>
                options.UseSqlite(connection));
#endif
            services.AddTransient<Bootstrap>();                                         // Used to seed user database
 
            // Add the Identity service

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddControllersWithViews();
            services.AddRazorPages();

            // Configure the Identity service options (eg for password complexity, etc)

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = Configuration.GetSection("AppSettings").GetValue<bool>("PasswordDigitRequired");
                options.Password.RequireLowercase = Configuration.GetSection("AppSettings").GetValue<bool>("PasswordLowercaseRequired");
                options.Password.RequireNonAlphanumeric = Configuration.GetSection("AppSettings").GetValue<bool>("PasswordSymbolRequired");
                options.Password.RequireUppercase = Configuration.GetSection("AppSettings").GetValue<bool>("PasswordUppercaseRequired");
                options.Password.RequiredLength = Configuration.GetSection("AppSettings").GetValue<int>("PasswordMinLength");
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = new TimeSpan
                                                                (
                                                                Configuration.GetSection("AppSettings").GetValue<int>("PasswordFailureLockoutHours"),
                                                                Configuration.GetSection("AppSettings").GetValue<int>("PasswordFailureLockoutMins"),
                                                                0
                                                                );
                options.Lockout.MaxFailedAccessAttempts = Configuration.GetSection("AppSettings").GetValue<int>("PasswordMaxFailures");
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            // Configure default paths for Login and Access Denied and other cookie-related options
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.LoginPath = new PathString(Configuration.GetSection("AppSettings").GetValue<string>("LoginPage"));
                options.AccessDeniedPath = new PathString(Configuration.GetSection("AppSettings").GetValue<string>("AccessDeniedPage"));
                options.SlidingExpiration = true;
            });

            // Create some policies related on requiring authenticated users being in a specific role
            services.AddAuthorization(options =>
            {
                var adminPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Configuration.GetSection("AppSettings").GetValue<string>("AdminRole")).RequireClaim(Configuration.GetSection("AppSettings").GetValue<string>("PasswordValidityClaim")).Build();
                var dataPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Configuration.GetSection("AppSettings").GetValue<string>("DatabaseRole")).RequireClaim(Configuration.GetSection("AppSettings").GetValue<string>("PasswordValidityClaim")).Build();
                options.AddPolicy(Configuration.GetSection("AppSettings").GetValue<string>("AdminPolicy"), adminPolicy);
                options.AddPolicy(Configuration.GetSection("AppSettings").GetValue<string>("DatabasePolicy"), dataPolicy);
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, Bootstrap bootstrapper)
        {
            List<string> additionalRoles = new List<string>();

            // Add the Database Event loggger

            loggerFactory.AddProvider(new DatabaseLoggerProvider(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>(), app.ApplicationServices));

            // If in development, use default exception page, otherwise use the page configured in the application settings

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                additionalRoles.Add(Configuration.GetSection("AppSettings").GetValue<string>("DatabaseRole"));

                bootstrapper.SeedUsersAndRoles                                                      // Potentially seed user database with initial data
                    (
                    Configuration.GetSection("AppSettings").GetValue<string>("AdminRole"),
                    additionalRoles,
                    Configuration.GetSection("AppSettings").GetValue<string>("InitialUser"),
                    Configuration.GetSection("AppSettings").GetValue<string>("InitialPassword")
                    );
            }
            else
            {
                app.UseExceptionHandler(Configuration.GetSection("AppSettings").GetValue<string>("DefaultErrorPage"));
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
