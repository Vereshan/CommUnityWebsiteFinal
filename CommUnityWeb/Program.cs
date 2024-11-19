using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Cookies; // Required for cookie authentication
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // Required for IHostEnvironment
using System;

namespace CommUnityWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register Firebase as a singleton
            builder.Services.AddSingleton<Firebase>(); // Replace 'Firebase' with your actual Firebase client type

            // Add HTTP client and session management services
            builder.Services.AddHttpClient();
            builder.Services.AddDistributedMemoryCache(); // Add distributed memory cache for session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            }); // Enable session management

            builder.Services.AddHttpContextAccessor(); // Register IHttpContextAccessor

            // Configure cookie authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login"; // Redirect here if not authenticated
                    options.LogoutPath = "/Home/Logout"; // Redirect here on logout
                    options.AccessDeniedPath = "/Home/AccessDenied"; // Redirect here if access denied
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set cookie expiration time
                    options.SlidingExpiration = true; // Renew the cookie on each request if close to expiration
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Use session before authentication and routing
            app.UseSession(); // Enable session support
            app.UseAuthentication(); // Enable authentication
            app.UseAuthorization(); // Enable authorization

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
