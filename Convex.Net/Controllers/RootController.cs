using Convex.Clients.Models;
using Microsoft.AspNetCore.Mvc;

namespace Convex.Clients.Controllers {
    public class RootController : Controller {
        private IClientHostedService Client { get; }

        public RootController(IClientHostedService clientHostedService) {
            Client = clientHostedService;
        }

        public IActionResult Index() {
            ViewData["Address"] = Client.Address;
            ViewData["IrcMessages"] = Client.GetAllMessages();

            return View();
        }
    }
}