using System.Linq;
using System.Web.Mvc;

namespace SmallId.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.AcceptTypes.Contains("application/xrds+xml"))
            {
                ViewData["OPIdentifier"] = true;
                return View("Xrds");
            }
            return View();
        }

        public ActionResult Xrds()
        {
            ViewData["OPIdentifier"] = true;
            return View();
        }
    }
}