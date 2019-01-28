using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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


        public ActionResult Chat()
        {
            List<string> temp = new List<string>();
            List<Group> query2 = new List<Group>();
            using (var context = new ApplicationDbContext())
            {
                using (var ctxtransaction = context.Database.BeginTransaction())
                {
                    string uname = User.Identity.GetUserId();
                    //   var userquery = context.Groups.SqlQuery("SELECT * FROM GroupUser").ToList();
                    //       Debug.Write(userquery);

                    //var query = context.Messages.Select(p => new { p.ID, p.messages });

                    query2 = context.Groups.SqlQuery("SELECT * FROM \"Group\" WHERE Group_Id IN (SELECT Group_Id FROM GroupUser WHERE User_Id LIKE '%" + User.Identity.GetUserId() + "%')").ToList();
                    for (int i = 0; i < query2.Count; i++)
                    {
                        temp.Add(query2[i].ID);
                    }
                    Debug.Write(query2);
                    //  var query = context.Users.SqlQuery("SELECT Group_Id FROM AspNetUsers").To
                    //  Debug.Write(query);
                }
            }
            return View(query2); ;
        }

        public ActionResult feedback()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        // when sending message

        public void sendMessage(string group, string messageIn)
        {
            string currentUser = User.Identity.GetUserId();
            using (var ctx = new ApplicationDbContext())

            {
                string esMessage = System.Security.SecurityElement.Escape(messageIn);
                List<Group> gp = new List<Group>();
                using (var ctxtrans = ctx.Database.BeginTransaction())
                {
                    gp = ctx.Groups.SqlQuery("SELECT * FROM \"Group\" WHERE Group_Id ='" + group + "'").ToList();
                    string msgjs = gp[0].GroupMessage;
                    List<Message> m = new List<Message>();


                    m = JsonConvert.DeserializeObject<List<Message>>(msgjs);
                    Message tmp = new Message();
                    tmp.message = esMessage;
                    tmp.ID = Convert.ToInt32(group) + 100;
                    tmp.User_Id = currentUser;
                    tmp.UserName = ctx.Users.Find(currentUser).UserName;
                    m.Add(tmp);
                    string after = JsonConvert.SerializeObject(m);
                    string sql = "UPDATE \"Group\" SET GroupMessage = @after WHERE Group_Id = @group";
                    SqlCommand command = new SqlCommand(sql);
                    command.Parameters.Add(new SqlParameter("@group", System.Data.SqlDbType.NVarChar));
                    command.Parameters.Add(new SqlParameter("@after", System.Data.SqlDbType.NVarChar));
                    command.Parameters["@group"].Value = group;
                    command.Parameters["@after"].Value = after;
                    // ctx.Database.ExecuteSqlCommand("UPDATE \"Group\" SET GroupMessage = '" + after + "' WHERE Group_Id = '" + group + "'");
                    CallDB(command);
                }
            }












            // return Index(Json(, JsonRequestBehavior.AllowGet));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult updateDB()
        {
            List<Group> a = new List<Group>();

            using (var ctx = new ApplicationDbContext())
            {
                string uname = User.Identity.GetUserId();
                var query3 = ctx.Groups.SqlQuery("SELECT * FROM \"Group\" WHERE Group_Id IN (SELECT Group_Id FROM GroupUser WHERE User_Id LIKE '%'" + User.Identity.GetUserId() + "'").ToList();
                string GroupID = query3[0].ID;
                a = ctx.Groups.SqlQuery("SELECT * \"Group\" WHERE Group_Id = '" + GroupID + "'").ToList();
                List<Message> m = new List<Message>();

                string ms = a[0].GroupMessage;
                m = JsonConvert.DeserializeObject<List<Message>>(a[0].GroupMessage);
                return Json(ms, JsonRequestBehavior.AllowGet);
            }

        }

        //after clicking on the gorup icons
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewGroups(string selected)
        {

            List<Group> query = new List<Group>();
            using (var ctx = new ApplicationDbContext())

            {
                //parameterinsing
                string sql = "SELECT * FROM \"Group\"  WHERE Group_Id = @selected_group";
                using (var ctxtrans = ctx.Database.BeginTransaction())
                {
                    string user = User.Identity.GetUserId();
                    SqlCommand command = new SqlCommand(sql);
                    command.Parameters.Add(new SqlParameter("@selected_group", System.Data.SqlDbType.NVarChar));
                    command.Parameters["@selected_group"].Value = selected;
                    //end parameterinsing
                    string a = selected;
                    Debug.Write(selected);
                    Debug.Write("AHDIADIOAHOIDHOAISHDSA");
                    query = ctx.Groups.SqlQuery(sql).ToList();
                    //   var q = ctx.Groups.Where(s => s.Group_Id == id).ToString();
                    Debug.Write(query);
                    string JSmessage = query[0].GroupMessage;
                    //string jsonmessage = query[0].GroupMessage;
                    return Json(JSmessage, JsonRequestBehavior.AllowGet);
                }

            }


        }
        // not part of code
        
        public ActionResult MessageGroup(string GroupSelected)
        {


            // List<Group> que;
            using (var ctx = new ApplicationDbContext())
            {
                using (var ctxtrans = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var query = ctx.Groups.SqlQuery("SELECT GroupMessage FROM \"Group\" WHERE Group_Id = '" + GroupSelected + "'");
                    }
                    catch (Exception)
                    {
                        ctxtrans.Rollback();
                    }
                }

            }
            return View("~/Views/App/Chat.cshtml");
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