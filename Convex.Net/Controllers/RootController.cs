using Convex.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace Convex.Client.Controllers {
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