using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
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

        // GET: /App/Feedback
        public ActionResult Feedback()
        {
            ViewBag.Message = "gib thee knowledge";
            return View();
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

                string uid = User.Identity.GetUserId();

                SqlCommand command = new SqlCommand(
                    "UPDATE AspNetUsers" +
                    "SET Private = @private" +
                    "WHERE ID= @id");

                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = uid
                };
                SqlParameter PVT = new SqlParameter
                {
                    ParameterName = "@private",
                    Value = data.Private
                };
                CallDB(command);
            }
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
                
                string uid = User.Identity.GetUserId();

                SqlCommand command = new SqlCommand(
                    "UPDATE AspNetUsers" +
                    "SET ProfileImg = @profileimg" +
                    "WHERE ID= @id");
                
                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = uid
                };
                SqlParameter IMG = new SqlParameter
                {
                    ParameterName = "@profilebio",
                    Value = data.ProfileImg
                };
                CallDB(command);
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

                string uid = User.Identity.GetUserId();

                SqlCommand command;
                if(data.Name.Equals("nme"))
                {
                    command = new SqlCommand(
                        "UPDATE AspNetUsers" +
                        "SET UserName = @username" +
                        "WHERE ID= @id");
                }
                else
                {
                    command = new SqlCommand(
                        "UPDATE AspNetUsers" +
                        "SET ProfileBio = @profilebio" +
                        "WHERE ID= @id");
                }
                
                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = uid
                };
                SqlParameter UNM = new SqlParameter
                {
                    ParameterName = "@username",
                    Value = data.Info
                };
                SqlParameter BIO = new SqlParameter
                {
                    ParameterName = "@profilebio",
                    Value = data.Info
                };
                CallDB(command);
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

        public static void CallDB(SqlCommand command)
        {
            using(SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
            {
                Debug.WriteLine(command.CommandText);
                conn.Open();
                command.Connection = conn;
                command.ExecuteNonQuery();
                conn.Close();
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