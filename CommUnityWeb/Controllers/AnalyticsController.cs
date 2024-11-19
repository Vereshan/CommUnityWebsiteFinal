using CommUnityWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityWeb.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly Firebase _firebase;

        public AnalyticsController()
        {
            _firebase = new Firebase();
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Index";
            var consumerData = await GetAnalyticsData("Consumer");
            var serviceProviderData = await GetAnalyticsData("ServiceProviders");

            var model = new List<AnalyticsViewModel> { consumerData, serviceProviderData };

            return View(model);
        }

        private async Task<AnalyticsViewModel> GetAnalyticsData(string collectionName)
        {
            var users = await _firebase.GetUsersByCollectionAsync(collectionName);

            var acceptedCount = users.Count(u =>  u.Status == "APPROVED");
            var declinedCount = users.Count(u => u.Status == "DECLINED");
            var pendingCount = users.Count(u => u.Status == "PENDING");

            return new AnalyticsViewModel
            {
                AcceptedCount = acceptedCount,
                DeclinedCount = declinedCount,
                PendingCount = pendingCount,
                CollectionName = collectionName
            };
        }

    }
}
