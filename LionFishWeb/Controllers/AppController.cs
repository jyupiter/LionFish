using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LionFishWeb.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public AppController()
        {
        }

        public AppController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
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
            ViewBag.ReturnUrl = returnUrl;
            User user = db.Users.Find(User.Identity.GetUserId());
            return View(user);
        }

        // POST: /App/UpdatePrivacy
        [HttpPost]
        [AllowAnonymous]
        public void UpdatePrivacy(UpdatePrivacyViewModel model)
        {
            if (ModelState.IsValid)
            {
                string json = new JavaScriptSerializer().Serialize(model);
                UpdatePrivacyViewModel data = JsonConvert.DeserializeObject<UpdatePrivacyViewModel>(json);

                using (var context = new ApplicationDbContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            context.Database.ExecuteSqlCommand(
                                @"UPDATE AspNetUsers" +
                                " SET Private = '" + data.Private + "'" +
                                " WHERE Id = '" + User.Identity.GetUserId() + "'"
                                );

                            context.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                }
            }
        }
        public ActionResult Feedback()
        {
            ViewBag.Message = "gib thee knowledge";
            return View();
        }
        // POST: /App/UpdateProfileImg
        [HttpPost]
        [AllowAnonymous]
        public void UpdateProfileImg(UpdateProfileImgViewModel model)
        {
            if (ModelState.IsValid)
            {
                string json = new JavaScriptSerializer().Serialize(model);
                UpdateProfileImgViewModel data = JsonConvert.DeserializeObject<UpdateProfileImgViewModel>(json);

                using (var context = new ApplicationDbContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            context.Database.ExecuteSqlCommand(
                                @"UPDATE AspNetUsers" +
                                " SET ProfileImg = '" + data.ProfileImg + "'" +
                                " WHERE Id = '" + User.Identity.GetUserId() + "'"
                                );

                            context.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                }
            }
        }

        // POST: /App/UpdateProfileInfo
        [HttpPost]
        [AllowAnonymous]
        public void UpdateProfileInfo(UpdateProfileInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                string json = new JavaScriptSerializer().Serialize(model);
                UpdateProfileInfoViewModel data = JsonConvert.DeserializeObject<UpdateProfileInfoViewModel>(json);

                using (var context = new ApplicationDbContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            if (data.Name.Equals("nme"))
                                context.Database.ExecuteSqlCommand(
                                    @"UPDATE AspNetUsers" +
                                    " SET UserName = '" + data.Info + "'" +
                                    " WHERE Id = '" + User.Identity.GetUserId() + "'"
                                    );
                            else
                                context.Database.ExecuteSqlCommand(
                                @"UPDATE AspNetUsers" +
                                " SET ProfileBio = '" + data.Info + "'" +
                                " WHERE Id = '" + User.Identity.GetUserId() + "'"
                                );

                            context.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                }
            }
        }

        // POST: /App/UpdatePassword
        [HttpPost]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task UpdatePasswordAsync(UpdatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            string json = new JavaScriptSerializer().Serialize(model);
            UpdatePasswordViewModel data = JsonConvert.DeserializeObject<UpdatePasswordViewModel>(json);

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), data.OldPassword, data.NewPassword);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
            }
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