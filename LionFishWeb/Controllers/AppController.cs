﻿using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LionFishWeb.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public AppController()
        {
        }

        public AppController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: /App/Dashboard
        [AllowAnonymous]
        public ActionResult Dashboard(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var userId = User.Identity.GetUserId();
            var model = new DashboardViewModel
            {
                ProfileImg = db.Users.Find(User.Identity.GetUserId()).ProfileImg
            };
            return View(model);
        }

        // GET: /App/Settings
        [AllowAnonymous]
        public ActionResult Settings(string returnUrl)
        {
            User user = db.Users.Find(User.Identity.GetUserId());
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(user);
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdatePrivacy(UpdatePrivacyViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = db.Users.Find(User.Identity.GetUserId());
                string json = new JavaScriptSerializer().Serialize(model);
                UpdatePrivacyViewModel data = JsonConvert.DeserializeObject<UpdatePrivacyViewModel>(json);
                user.Private = data.Private;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Json(model);
        }

        #region Helpers
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }
        #endregion

    }
}