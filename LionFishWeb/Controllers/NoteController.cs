using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;

namespace LionFishWeb.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        private static Note currentNote;

        public NoteController()
        {
        }

        public NoteController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        public void SetCurrentNote(SetCurrentNoteViewModel model)
        {
            using(SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Note WHERE ID = @id", conn);
                SqlParameter NID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = model.Id
                };
                command.Parameters.Add(NID);

                Debug.WriteLine(command.CommandText);

                conn.Open();
                currentNote = (Note)command.ExecuteScalar();
                conn.Close();
            }
        }

        public Note GetCurrentNote()
        {
            return currentNote;
        }

        // GET: Note/Index
        public ActionResult Index()
        {
            NoteFolderViewModel NFVM = new NoteFolderViewModel();
            NFVM.NVM.Notes = Load(User.Identity.GetUserId());
            NFVM.FVM.Folders = Load(User.Identity.GetUserId(), "");
            NFVM.Requested = GetCurrentNote();
            return View("Index", NFVM);
        }

        public static List<Note> Load(string id)
        {
            List<Note> nl = new List<Note>();
            using(SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Note WHERE UserID = @userid", conn);
                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@userid",
                    Value = id
                };
                command.Parameters.Add(UID);

                Debug.WriteLine(command.CommandText);

                conn.Open();
                using(SqlDataReader r = command.ExecuteReader())
                {
                    while(r.Read())
                    {
                        Note n = new Note
                        {
                            ID = r["ID"].ToString(),
                            Title = r["Title"].ToString(),
                            Content = r["Content"].ToString(),
                            FolderID = r["FolderID"].ToString(),
                            UserID = id,
                            EventID = r["EventID"].ToString()
                        };
                        nl.Add(n);
                    }
                }
                conn.Close();
            }
            return nl;
        }

        public static List<Folder> Load(string id, string _)
        {
            List<Folder> fl = new List<Folder>();
            using(SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Folder WHERE UserID = @userid", conn);
                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@userid",
                    Value = id
                };
                command.Parameters.Add(UID);

                Debug.WriteLine(command.CommandText);

                conn.Open();
                using(SqlDataReader r = command.ExecuteReader())
                {
                    while(r.Read())
                    {
                        Folder f = new Folder
                        {
                            ID = r["ID"].ToString(),
                            Name = r["Name"].ToString(),
                            UserID = id
                        };
                        fl.Add(f);
                    }
                }
                conn.Close();
            }
            return fl;
        }

        public static List<Event> Load(string id, string _, string __)
        {
            List<Event> el = new List<Event>();
            using(SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Event WHERE ID = @id", conn);
                SqlParameter EID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = id
                };
                command.Parameters.Add(EID);

                Debug.WriteLine(command.CommandText);

                conn.Open();
                using(SqlDataReader r = command.ExecuteReader())
                {
                    while(r.Read())
                    {
                        Event f = new Event
                        {
                            ID = r["ID"].ToString(),
                            Title = r["Title"].ToString(),
                            Description = r["Description"].ToString(),
                            AllDay = Convert.ToBoolean(r["AllDay"].ToString()),
                            Start = Convert.ToDateTime(r["Start"].ToString()),
                            End = Convert.ToDateTime(r["End"].ToString()),
                            Color = r["Color"].ToString(),
                            UserID = id,
                            Public = Convert.ToBoolean(r["Public"].ToString())
                        };
                        el.Add(f);
                    }
                }
                conn.Close();
            }
            return el;
        }

        public JsonResult GetNoteDetails(string ID)
        {
            Note n = new Note();
            List<Note> notes = Load(User.Identity.GetUserId());
            foreach(Note nt in notes)
            {
                if(nt.ID == ID)
                {
                    n = nt;
                }
            }
            return Json(n, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFolderName(string ID)
        {
            Folder f = new Folder();
            List<Folder> folders = Load(User.Identity.GetUserId(), "");
            foreach(Folder fl in folders)
            {
                if(fl.ID == ID)
                {
                    f = fl;
                }
            }
            return Json(f.Name, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEventTitle(string ID)
        {
            List<Event> events = Load(ID, "", "");
            return Json(events[0].Title, JsonRequestBehavior.AllowGet);
        }

        // POST: /Note/CreateNote
        [HttpPost]
        [AllowAnonymous]
        public void CreateNote(CreateNoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uid = User.Identity.GetUserId();
                Folder f = Load(uid, "")[0];
                Note n = new Note();
                SqlCommand command = new SqlCommand(
                    "INSERT INTO Note (ID, Title, Content, FolderID, UserID)" +
                    "VALUES (@id, @title, @content, @folderid, @userid)");
                SqlParameter NID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = n.ID
                };
                SqlParameter TTL = new SqlParameter
                {
                    ParameterName = "@title",
                    Value = model.Title
                };
                SqlParameter CTT = new SqlParameter
                {
                    ParameterName = "@content",
                    Value = n.Content
                };
                SqlParameter FID = new SqlParameter
                {
                    ParameterName = "@folderid",
                    Value = f.ID
                };
                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@userid",
                    Value = uid
                };
                CallDB(command);
            }
        }

        // POST: /Note/CreateFolder
        [HttpPost]
        [AllowAnonymous]
        public void CreateFolder(CreateFolderViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uid = User.Identity.GetUserId();
                Folder f = new Folder();
                SqlCommand command = new SqlCommand(
                    "INSERT INTO Folder (ID, Name, UserID)" +
                    "VALUES (@id, @name, @userid)");
                SqlParameter FID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = f.ID
                };
                SqlParameter NME = new SqlParameter
                {
                    ParameterName = "@name",
                    Value = model.Name
                };
                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@userid",
                    Value = uid
                };
                CallDB(command);
            }
        }

        // POST: /Note/UpdateNote
        [HttpPost]
        [AllowAnonymous]
        public void UpdateNote(UpdateNoteViewModel model)
        {
            if(ModelState.IsValid)
            {
                SqlCommand command = new SqlCommand(
                    "UPDATE Note" +
                    "SET Title = @title, Content = @content" +
                    "WHERE ID= @id");
                SqlParameter NID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = model.ID
                };
                SqlParameter TTL = new SqlParameter
                {
                    ParameterName = "@title",
                    Value = model.Title
                };
                SqlParameter CTT = new SqlParameter
                {
                    ParameterName = "@content",
                    Value = model.Content
                };
                CallDB(command);
            }
        }

        public static void CallDB(SqlCommand command)
        {
            using(SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
            {
                Debug.WriteLine(command.CommandText);
                conn.Open();
                command.Connection = conn;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception) { }
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