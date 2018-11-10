using Microsoft.AspNetCore.Mvc;

namespace Convex.Client.Controllers {
    public class HomeController : Controller {
        //
        // GET: /Home/
        public ActionResult Index() {
            return View();
        }
    }
}
