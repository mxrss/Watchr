using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignalRGateway.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
         
            return View();
        }

        public ActionResult Watch(Guid id)
        {
            ViewBag.id = id;
            return View();
        }
    }
}