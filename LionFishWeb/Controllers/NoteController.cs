using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;
using LionFishWeb.Utility;

namespace LionFishWeb.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
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
                string uid = User.Identity.GetUserId();
                Note n = new Note();
                SqlCommand command = new SqlCommand("SELECT * FROM Note WHERE ID = @id", conn);
                SqlParameter NID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = model.Id.GetDirectReference()
                };
                command.Parameters.Add(NID);

                conn.Open();
                using(SqlDataReader r = command.ExecuteReader())
                {
                    while(r.Read())
                    {
                        n = new Note
                        {
                            ID = model.Id,
                            Title = r["Title"].ToString(),
                            Content = r["Content"].ToString(),
                            FolderID = r["FolderID"].ToString().GetIndirectReference(),
                            UserID = uid.GetIndirectReference(),
                            EventID = r["EventID"].ToString().GetIndirectReference()
                        };
                    }
                }
                conn.Close();
                currentNote = n;
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
                            ID = r["ID"].ToString().GetIndirectReference(),
                            Title = r["Title"].ToString(),
                            Content = r["Content"].ToString(),
                            FolderID = r["FolderID"].ToString().GetIndirectReference(),
                            UserID = id.GetIndirectReference(),
                            EventID = r["EventID"].ToString().GetIndirectReference()
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
                            ID = r["ID"].ToString().GetIndirectReference(),
                            Name = r["Name"].ToString(),
                            UserID = id.GetIndirectReference()
                        };
                        fl.Add(f);
                    }
                }
                conn.Close();
            }
            return fl;
        }

        public static Event Load(string id, string _, string __)
        {
            Event e = new Event();
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
                        e = new Event
                        {
                            ID = r["ID"].ToString().GetIndirectReference(),
                            Title = r["Title"].ToString(),
                            Description = r["Description"].ToString(),
                            AllDay = Convert.ToBoolean(r["AllDay"].ToString()),
                            Start = Convert.ToDateTime(r["Start"].ToString()),
                            End = Convert.ToDateTime(r["End"].ToString()),
                            Color = r["Color"].ToString(),
                            UserID = id.GetIndirectReference(),
                            Public = Convert.ToBoolean(r["Public"].ToString())
                        };
                    }
                }
                conn.Close();
            }
            return e;
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
                    break;
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
                    break;
                }
            }
            return Json(f.Name, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEventTitle(string ID)
        {
            Event e = Load(ID.GetDirectReference(), "", "");
            return Json(e.Title, JsonRequestBehavior.AllowGet);
        }

        // POST: /Note/CreateNote
        [HttpPost]
        [AllowAnonymous]
        public void CreateNote(CreateNoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uid = User.Identity.GetUserId();
                List<Folder> fl = Load(uid, "");
                Folder f = new Folder();
                foreach(Folder x in fl)
                {
                    if(x.Name == model.Name && x.UserID.GetDirectReference() == uid)
                    {
                        Debug.WriteLine(x.ID);
                        f = x;
                        break;
                    }
                }
                Note n = new Note();
                SqlCommand command = new SqlCommand(
                    "INSERT INTO Note (ID, Title, Content, FolderID, UserID) " +
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
                    Value = f.ID.GetDirectReference()
                };
                SqlParameter UID = new SqlParameter
                {
                    ParameterName = "@userid",
                    Value = uid
                };
                command.Parameters.Add(NID);
                command.Parameters.Add(TTL);
                command.Parameters.Add(CTT);
                command.Parameters.Add(FID);
                command.Parameters.Add(UID);
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
                    "INSERT INTO Folder (ID, Name, UserID) " +
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
                command.Parameters.Add(FID);
                command.Parameters.Add(NME);
                command.Parameters.Add(UID);
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
                    "UPDATE Note " +
                    "SET Title = @title, Content = @content " +
                    "WHERE ID= @id");
                SqlParameter NID = new SqlParameter
                {
                    ParameterName = "@id",
                    Value = model.ID.GetDirectReference()
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
                command.Parameters.Add(NID);
                command.Parameters.Add(TTL);
                command.Parameters.Add(CTT);
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