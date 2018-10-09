using Convex.Clients.Models;
using Microsoft.AspNetCore.Mvc;

namespace Convex.Clients__Server_.Controllers {
    public class RootController : Controller {
        private IIrcClient Client { get; }

        public RootController() {
            Client = new IrcClient("irc.foonetic.net", 6667);
        }

        public IActionResult Index() {
            ViewData["Address"] = Client.Address;
            ViewData["IrcMessages"] = Client.GetAllMessages();

            return View();
        }
    }
}