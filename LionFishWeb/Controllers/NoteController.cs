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

        // GET: Note
        public ActionResult Index()
        {
            NoteFolderViewModel NFVM = new NoteFolderViewModel();
            NFVM.NVM.Notes = Load();
            NFVM.FVM.Folders = Load("");
            return View(NFVM);
        }

        public static List<Note> Load()
        {
            using (var context = new ApplicationDbContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var query = context.Notes.SqlQuery("SELECT * FROM Note").ToList<Note>();
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

        public static List<Folder> Load(string placeholder)
        {
            using (var context = new ApplicationDbContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var query = context.Folders.SqlQuery("SELECT * FROM Folder").ToList<Folder>();
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

        // POST: /Note/CreateNote
        [HttpPost]
        [AllowAnonymous]
        public void CreateNote(CreateNoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                string json = new JavaScriptSerializer().Serialize(model);
                CreateNoteViewModel data = JsonConvert.DeserializeObject<CreateNoteViewModel>(json);

                using (var context = new ApplicationDbContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var query = context.Folders.Where(p => p.Name == model.Name);
                            Debug.WriteLine(query.ToArray());
                            
                            var q = query.ToArray();
                            context.Database.ExecuteSqlCommand(
                                @"INSERT INTO Note (Title, FolderID, UserID) " +
                                 "VALUES ('" + model.Title + "', '" + q[0].ID + "', '" + User.Identity.GetUserId() + "');"
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
                string json = new JavaScriptSerializer().Serialize(model);
                CreateFolderViewModel data = JsonConvert.DeserializeObject<CreateFolderViewModel>(json);

                using (var context = new ApplicationDbContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            context.Database.ExecuteSqlCommand(
                                @"INSERT INTO Folder (Name, UserID) " +
                                 "VALUES ('" + model.Name + "', '" + User.Identity.GetUserId() + "');"
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