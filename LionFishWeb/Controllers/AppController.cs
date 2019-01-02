using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LionFishWeb.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        // GET: /App/Dashboard
        [AllowAnonymous]
        public ActionResult Dashboard(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /App/Settings
        [AllowAnonymous]
        public ActionResult Settings(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}