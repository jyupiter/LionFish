using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using LionFishWeb.Models;

using static LionFishWeb.Models.GroupViewModels;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace LionFishWeb.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public GroupsController() { }
        public GroupsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: /Groups
        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            GroupsViewModel group = new GroupsViewModel();
            ViewBag.ReturnUrl = returnUrl;
            List<Group> listOfGroups = new List<Group>();
            listOfGroups = Load();

            foreach(Group grouperdoodles in listOfGroups)
            {
                ICollection<User> sersuse = GetUserListFromGroup(grouperdoodles.ID);
                grouperdoodles.User = sersuse;
            };

            ViewData["Groups"] = listOfGroups;
            group.Grouped = listOfGroups;
            Debug.WriteLine("Index Called");
            return View(group);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(GroupsViewModel group, string returnUrl)
        {

            ViewBag.ReturnUrl = returnUrl;
            CreateGroup(group);
            List<Group> listOfGroups;
            listOfGroups = Load();

            group.Grouped = listOfGroups;
            Debug.WriteLine("Index Called");
            return View(group);
        }


        public List<Group> Load()
        {

            string commandText = "SELECT * FROM[dbo].[Group] where ID IN (SELECT Group_ID FROM[dbo].[UserGroup] where User_Id IN (SELECT Id FROM[dbo].[AspNetUsers] where Id = '" + User.Identity.GetUserId() + "') );";
            List<Group> groups = new List<Group>();

            using(SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using(SqlCommand dbCommand = new SqlCommand(commandText, dbConnection))
            {
                dbCommand.CommandType = System.Data.CommandType.Text;
                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                while(reader.Read())
                {
                    groups.Add(new Group()
                    {
                        ID = (string)reader["ID"],
                        GroupName = (string)reader["GroupName"],
                        GroupDesc = (string)reader["GroupDesc"],
                        GroupImage = (string)reader["GroupImage"],
                        Owner_Id = (string)reader["Owner_Id"]

                    });

                }
                dbConnection.Close();
            }

            return groups;
        }

        public void CreateGroup(GroupsViewModel model)
        {
            string named = model.Groups.GroupName;
            string desc = model.Groups.GroupDesc;
            string image = model.Groups.GroupImage;
            string idd = "" + Guid.NewGuid();

            Boolean peekaboo = true;

            string commandText = "INSERT INTO [dbo].[Group] (Id, GroupName, GroupDesc, GroupImage,Owner_Id) VALUES ('" + idd + "','" + named + "','" + desc + "','" + image + "','" + User.Identity.GetUserId() + "');";
            string commandText2 = "INSERT INTO [dbo].[UserGroup] (User_Id, Group_ID) VALUES ('" + User.Identity.GetUserId() + "' , '" + idd + "');";
            using(SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using(SqlCommand dbCommand = new SqlCommand(commandText, dbConnection))
            {
                dbCommand.CommandType = System.Data.CommandType.Text;
                dbConnection.Open();
                try
                { dbCommand.ExecuteNonQuery(); }
                catch(Exception o) { peekaboo = false; Debug.WriteLine(o.StackTrace); }

                if(peekaboo == true)
                {
                    SqlCommand dbCommand2 = new SqlCommand(commandText2, dbConnection);
                    dbCommand2.ExecuteNonQuery();
                }
                else
                { }

                dbConnection.Close();
            }


        }
        public ICollection<User> GetUserListFromGroup(string groupid)
        {
            string commandText = "SELECT User_Id FROM [dbo].[UserGroup] WHERE Group_ID = '" + groupid + "';";
            List<string> userid = new List<string>();
            List<string> usernames = new List<string>();
            List<User> listouser = new List<User>();
            Boolean peekaboo = true;

            using(SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using(SqlCommand dbCommand = new SqlCommand(commandText, dbConnection))
            {
                dbConnection.Open();
                try
                {
                    SqlDataReader reader = dbCommand.ExecuteReader();
                    while(reader.Read())
                    {
                        userid.Add((string)reader["User_Id"]);
                    }
                }
                catch(Exception e) { peekaboo = false; }
            }
            if(peekaboo)
            {
                foreach(string usered in userid)
                {
                    SqlConnection dbConnection2 = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                    string commandText2 = "SELECT * FROM [dbo].[AspNetUsers] WHERE (Id) = '" + usered + "';";
                    SqlCommand dbCommand2 = new SqlCommand(commandText2, dbConnection2);

                    dbConnection2.Open();
                    SqlDataReader reader2 = dbCommand2.ExecuteReader();
                    // Can insert try catch here
                    while(reader2.Read())
                    {
                        listouser.Add(new User()
                        {
                            //Add whatever you need here, I add the main reading stuff here.
                            Id = (string)reader2["Id"],
                            UserName = (string)reader2["UserName"],
                            Email = (string)reader2["Email"],
                            ProfileBio = (reader2["ProfileBio"] == DBNull.Value) ? string.Empty : (string)reader2["ProfileBio"],
                            ProfileImg = (reader2["ProfileImg"] == DBNull.Value) ? string.Empty : (string)reader2["ProfileImg"],
                            //AccessFailedCount = (reader2["AccessFailedCount"] == DBNull.Value) ? int.MinValue : (int)reader2["ProfileImg"],
                            //Private = (reader2["ProfileImg"] == DBNull.Value) ? string.Empty : (string)reader2["ProfileImg"],
                            //EmailConfirmed = (reader2["EmailConfirmed"] == DBNull.Value) ? true : (bool)reader2["EmailConfirmed"],
                            //Events = (reader2["Events"] == DBNull.Value) ? null : (ICollection<Event>)reader2["Events"],
                            //LockoutEnabled = true

                        });

                        usernames.Add((string)reader2["UserName"])
                            ;
                    }
                    dbConnection2.Close();
                }
            }
            else
            { return null; }



            ViewData["Users"] = listouser;
            return listouser;
        }

        public ActionResult InviteMembers(GroupViewModel model)
        {

            string ReceiveResult = model.search.Search;
            string idd = GetUserIdfromName(ReceiveResult);
            Group grouped = model.group;
            string commandText2 = "INSERT INTO [dbo].[UserGroup] (User_Id, Group_ID) VALUES ('" + idd + "' , '" + grouped.ID + "');";

            using(SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using(SqlCommand dbCommand = new SqlCommand(commandText2, dbConnection))
            {
                dbCommand.CommandType = System.Data.CommandType.Text;
                dbConnection.Open();
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch(Exception o)
                { }
                dbConnection.Close();
            };
            return null;
        }
        [HttpGet]
        public List<string> GetSearchMembers(string name)
        {

            List<string> neo = new List<string>();

            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlCommand command = new SqlCommand("SELECT (Username) FROM [dbo].[AspNetUsers] WHERE Username LIKE '%" + name + "%';", dbConnection);
            dbConnection.Open();
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    string potato = (string)reader["Username"];
                    neo.Add(potato);
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }


            return neo;
        }

        [HttpPost]
        public JsonResult GetSearchingData(string SearchValue)
        {
            List<User> StuList = new List<User>();
            if(SearchValue != "")
            {
                SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                SqlCommand command = new SqlCommand("SELECT * FROM [dbo].[AspNetUsers] WHERE Username LIKE '" + SearchValue + "%';", dbConnection);
                //command.Parameters.Add("@UserName", SqlDbType.NVarChar);
                //command.Parameters["@UserName"].Value = SearchValue;


                dbConnection.Open();
                try
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        StuList.Add(new User()
                        {
                            UserName = (string)reader["UserName"],
                        });
                    }
                }
                catch(FormatException)
                {
                    Console.WriteLine("No results found.");
                }
                return Json(StuList, JsonRequestBehavior.AllowGet);



            }
            else
            {
                List<User> hello = new List<User>();
                return Json(hello, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateGroups(string name, string data, string grouped)
        {
            string commandText;
            if(name.Equals("GroupName"))
            {
                commandText = "UPDATE [dbo].[Group] SET GroupName = '" + data + "' WHERE ID ='" + grouped + "';";
                ;
            }
            else
            {
                commandText = "UPDATE [dbo].[Group] SET GroupDesc = '" + data + "' WHERE ID ='" + grouped + "';";
            }

            using(SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using(SqlCommand dbCommand = new SqlCommand(commandText, dbConnection))
            {
                dbConnection.Open();
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
                dbConnection.Close();
            }

            return View();
        }

        [HttpPost]
        public ActionResult EditGroup(string id)
        {
            string commandText = "SELECT * FROM[dbo].[Group] where ID = '" + id + "';";
            List<Group> groups = new List<Group>();
            GroupsViewModel model = new GroupsViewModel();

            using(SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using(SqlCommand dbCommand = new SqlCommand(commandText, dbConnection))
            {
                dbCommand.CommandType = System.Data.CommandType.Text;
                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                while(reader.Read())
                {
                    groups.Add(new Group()
                    {
                        ID = (string)reader["ID"],
                        GroupName = (string)reader["GroupName"],
                        GroupDesc = (string)reader["GroupDesc"],
                        GroupImage = (string)reader["GroupImage"]
                    });

                }
                dbConnection.Close();
            }
            ViewData["CurrentGroup"] = groups;
            ViewBag.ReturnUrl = id;
            return View();
        }

        // GET /Groups/Group
        public ActionResult Group(Group submodel, string returnUrl)
        {
            ViewBag.returnUrl = returnUrl + submodel.ID;
            ICollection<User> userlist = GetUserListFromGroup(submodel.ID);
            submodel.User = userlist;
            GroupViewModel model = new GroupViewModel();
            model.group = submodel;
            return View(model);
        }

        public ActionResult UpdateGroupDesc(Group group)
        {
            string commandText = "UPDATE [dbo].[Group] SET GroupDesc = '" + group.GroupDesc + "');";
            using(SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using(SqlCommand dbCommand = new SqlCommand(commandText, dbConnection))
            {
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
            }

            return View();
        }

        public string GetUserIdfromName(string username)
        {
            string potato = "";
            SqlConnection dbConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlCommand command = new SqlCommand("SELECT (Id) FROM [dbo].[AspNetUsers] WHERE Username ='" + username + "';", dbConnection);
            dbConnection.Open();
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    potato = (string)reader["Id"];
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }

            return potato;
        }
    }
}