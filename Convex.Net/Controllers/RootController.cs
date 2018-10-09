using Convex.Clients.Models;
using Microsoft.AspNetCore.Mvc;

namespace Convex.Clients__Server_.Controllers {
    public class RootController : Controller {
        private IIrcClient Client { get; }

        public RootController(IIrcClient client) {
            Client = client;
        }

        public IActionResult Index() {
            ViewData["Address"] = Client.Address;
            ViewData["IrcMessages"] = Client.GetAllMessages();
            ViewData

            return View();
        }
    }
}