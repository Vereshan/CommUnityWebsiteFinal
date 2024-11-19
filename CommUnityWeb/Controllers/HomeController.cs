using CommUnityWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly Firebase _firebase;

    public HomeController()
    {
        _firebase = new Firebase();
    }

    // Display login view
    public IActionResult Login()
    {
        ViewData["ActivePage"] = "login";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(UserLoginModel loginModel)
    {
        // Call the authentication method
        var (user, errorMessage) = await _firebase.AuthenticateAdminAsync(loginModel.Email, loginModel.Password);

        if (user != null)
        {
            // Create the user identity and claim
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to dashboard after successful login
            return RedirectToAction("Index", "Analytics");
        }

        // If there is an error message, display it
        TempData["ErrorMessage"] = errorMessage ?? "Invalid login attempt";
        return RedirectToAction("Login");
    }

    // Logout action
    public IActionResult Logout()
    {
        // Clear session
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }


}
