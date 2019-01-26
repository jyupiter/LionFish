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
            using(var context = new ApplicationDbContext())
            {
                using(var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var query = context.Notes.SqlQuery("SELECT * FROM Note WHERE ID ='" + model.Id + "'").ToList<Note>();
                        currentNote = query[0];
                        Debug.WriteLine(currentNote);
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
        }

        public Note GetCurrentNote()
        {
            return currentNote;
        }

        // GET: Note
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
            using (var context = new ApplicationDbContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var query = context.Notes.SqlQuery("SELECT * FROM Note WHERE UserID ='" + id + "'").ToList<Note>();
                        return query;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            return null;
        }

        public static List<Folder> Load(string id, string _)
        {
            using (var context = new ApplicationDbContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var query = context.Folders.SqlQuery("SELECT * FROM Folder WHERE UserID ='" + id + "'").ToList<Folder>();
                        return query;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            return null;
        }

        public static List<Event> Load(string id, string _, string __)
        {
            using(var context = new ApplicationDbContext())
            {
                using(var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var query = context.Events.SqlQuery("SELECT * FROM Event WHERE ID ='" + id + "'").ToList<Event>();
                        return query;
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            return null;
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
                using (var context = new ApplicationDbContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            string uid = User.Identity.GetUserId();
                            var query = context.Folders.Where(p => p.Name == model.Name && p.UserID == uid);


                            Note n = new Note();
                            var q = query.ToArray();
                            context.Database.ExecuteSqlCommand(
                                @"INSERT INTO Note (ID, Title, Content, FolderID, UserID) " +
                                 "VALUES ('" + n.ID + "', '" + model.Title + "', '" + n.Content + "', '" + q[0].ID + "', '" + uid + "');"
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

        // POST: /Note/CreateFolder
        [HttpPost]
        [AllowAnonymous]
        public void CreateFolder(CreateFolderViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new ApplicationDbContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            Folder f = new Folder();
                            context.Database.ExecuteSqlCommand(
                                @"INSERT INTO Folder (ID, Name, UserID) " +
                                "VALUES ('" + f.ID + "', '" + model.Name + "', '" + User.Identity.GetUserId() + "');"
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

        // POST: /Note/UpdateNote
        [HttpPost]
        [AllowAnonymous]
        public void UpdateNote(UpdateNoteViewModel model)
        {
            if(ModelState.IsValid)
            {
                using(var context = new ApplicationDbContext())
                {
                    using(var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            context.Database.ExecuteSqlCommand(
                                @"UPDATE Note" +
                                " SET Title = '" + model.Title + "', Content = '" + model.Content + "'" +
                                " WHERE Id = '" + model.ID + "'"
                            );

                            context.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        catch(Exception)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
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