using Microsoft.AspNetCore.Mvc;
using CommUnityWeb.Models;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace CommUnityWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly Firebase _firebase;

        public AdminController()
        {
            _firebase = new Firebase(); // Instantiate Firebase service
        }

        public IActionResult Index()
        {
            return View();
        }

        //All Methods for Service Providers

        public async Task<IActionResult> ReviewDashboard()
        {
            ViewData["ActivePage"] = "Service_Provider_Dashboard";
            var users = await _firebase.GetPendingServiceProvidersAsync(); // Fetch pending users
            Console.WriteLine($"Passing {users.Count()} users to the view.");
            return View(users); // Pass users to the view
        }

        public async Task<IActionResult> VerifyServiceProvider(string id) // Changed from 'IDNumber' to 'id'
        {
            // Fetch user information based on the provided ID
            var user = await _firebase.GetServiceProviderByIdNumberAsync(id); // Use 'id' here as well

            if (user == null)
            {
                return NotFound(); // Handle the case when the user is not found
            }

            return View(user); // Pass the user object to the Verify view
        }

        public async Task<IActionResult> ApproveServiceProvider(string id)
        {
            var isUpdated = await _firebase.UpdateUserStatusAsync(id, "APPROVED", "ServiceProvider");

            if (isUpdated)
            {
                // Get the consumer's details, including FCM token
                var serviceProvider = await _firebase.GetServiceProviderByIdNumberAsync(id);

                if (serviceProvider != null && !string.IsNullOrEmpty(serviceProvider.FcmToken))
                {
                    await _firebase.SendNotificationAsync(
                        serviceProvider.FcmToken,
                        "Registration Approved\u2705",
                        "Your registration has been approved as a service provider☺."
                    );
                }

                return RedirectToAction("ReviewDashboard"); // Redirect after approval
            }
            else
            {
                return BadRequest("Unable to update the status.");
            }
        }

        public async Task<IActionResult> DeclineServiceProvider(string id)
        {
            var isUpdated = await _firebase.UpdateUserStatusAsync(id, "DECLINED", "ServiceProvider");

            if (isUpdated)
            {
                // Get the consumer's details, including FCM token
                var serviceProvider = await _firebase.GetServiceProviderByIdNumberAsync(id);

                if (serviceProvider != null && !string.IsNullOrEmpty(serviceProvider.FcmToken))
                {
                    await _firebase.SendNotificationAsync(
                        serviceProvider.FcmToken,
                        "Registration Declined\u274C",
                        "Your registration has been declined\u2639."
                    );
                }

                return RedirectToAction("ReviewDashboard"); // Redirect after decline
            }
            else
            {
                return BadRequest("Unable to update the status.");
            }
        }


        //All methods for consumers
        public async Task<IActionResult> ConsumerDashboard()
        {
            ViewData["ActivePage"] = "Consumer_Dashboard";
            var users = await _firebase.GetPendingConsumersAsync(); // Fetch pending users
            Console.WriteLine($"Passing {users.Count()} users to the view.");
            return View(users); // Pass users to the view        
        }

        public async Task<IActionResult> VerifyConsumer(string id) // Changed from 'IDNumber' to 'id'
        {
            // Fetch user information based on the provided ID
            var user = await _firebase.GetConsumerByIdNumberAsync(id); // Use 'id' here as well

            if (user == null)
            {
                return NotFound(); // Handle the case when the user is not found
            }

            return View(user); // Pass the user object to the Verify view
        }

        public async Task<IActionResult> ApproveConsumer(string id)
        {
            var isUpdated = await _firebase.UpdateUserStatusAsync(id, "APPROVED", "Consumer");

            if (isUpdated)
            {
                // Get the consumer's details, including FCM token
                var consumer = await _firebase.GetConsumerByIdNumberAsync(id);

                if (consumer != null && !string.IsNullOrEmpty(consumer.FcmToken))
                {
                    await _firebase.SendNotificationAsync(
                        consumer.FcmToken,
                        "Registration Approved\u2705",
                        "Your registration has been approved as a consumer \u263A."
                    );
                }

                return RedirectToAction("ConsumerDashboard"); // Redirect after approval
            }
            else
            {
                return BadRequest("Unable to update the status.");
            }
        }

        public async Task<IActionResult> DeclineConsumer(string id)
        {
            var isUpdated = await _firebase.UpdateUserStatusAsync(id, "DECLINED", "Consumer");

            if (isUpdated)
            {
                // Get the consumer's details, including FCM token
                var consumer = await _firebase.GetConsumerByIdNumberAsync(id);

                if (consumer != null && !string.IsNullOrEmpty(consumer.FcmToken))
                {
                    await _firebase.SendNotificationAsync(
                        consumer.FcmToken,
                        "Registration Declined\u274C",
                        "Your registration has been declined\u2639."
                    );
                }

                return RedirectToAction("ConsumerDashboard"); // Redirect after decline
            }
            else
            {
                return BadRequest("Unable to update the status.");
            }
        }


        // Step 3: Method to display details for both consumers and service providers
        public async Task<IActionResult> ViewDetails(string id, string userType)
        {
            // Fetch user details based on the type (either 'Consumer' or 'ServiceProvider')
            object user;
            if (userType == "ServiceProvider")
            {
                user = await _firebase.GetServiceProviderByIdNumberAsync(id);
            }
            else if (userType == "Consumer")
            {
                user = await _firebase.GetConsumerByIdNumberAsync(id);
            }
            else
            {
                return BadRequest("Invalid user type specified."); // Handle invalid type
            }

            if (user == null)
            {
                return NotFound(); // Handle the case when the user is not found
            }

            return View(user); // Pass the user object to the view
        }

    

    }
}

//https://www.youtube.com/watch?v=nqIX4Oe25fk
