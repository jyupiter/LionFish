using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LionFishWeb.Controllers
{
    public class CalendarController : Controller
	{
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;

		public CalendarController()
		{
		}

		public CalendarController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

		// GET: Calendar
		public const string path = "D:\\Github\\LionFishWeb\\Events";

		public ActionResult Calendar()
		{
			List<Event> listOfEvents = new List<Event>();
			listOfEvents = Load();
			ViewData["Events"] = listOfEvents;
			Debug.WriteLine("calendar() called");
			return View();

			//string[] filePaths = Directory.GetFiles(path);
			//List<Event> listOfEvents = new List<Event>();
			//foreach (string file in filePaths)
			//{
			//	Event events = Event.UnJson(System.IO.File.ReadAllText(file));
			//	listOfEvents.Add(events);
			//}
			//ViewData["Events"] = listOfEvents;
			//Debug.WriteLine("calendar() called");
			//return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Apply([Bind(Include = "ID, Title, Color, Description, AllDay, Start, End")] Event events)
		{
			Debug.WriteLine(events.Title);
			Save(events, "update", events.ID);
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "Title, Color, Description, AllDay, Start, End")] Event events)
		{
			Debug.WriteLine(events.Title);
			Save(events, "create", "0");
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete([Bind(Include = "ID")] Event events)
		{
			Debug.WriteLine(events.ID);
			Save(events, "delete", events.ID);
			return RedirectToAction("Calendar");
		}

		public static List<Event> Load()
		{
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						var query = context.Events.SqlQuery("SELECT ID,Title,Description,AllDay,\"Start\",\"end\",Color FROM Event").ToList<Event>();
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

		public static void Save(Event events , string mode, string id) {
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						var query = context.Users.Where(p => p.Email == "xxxfiredragonmcxxx@gmail.com");
						//string id = User.Identity.GetUserId();
						var query2 = query.ToArray();
						Debug.WriteLine(query2[0].Id);
						context.Database.ExecuteSqlCommand(
								@"UPDATE AspNetUsers" +
								" SET User_Id = '" + query2[0].Id + "'" +
								" WHERE Id = '" + query2[0].Id + "'"
								);

						if (mode == "create"){
							Debug.WriteLine("INSERT INTO Event (ID,Title,Description,AllDay,Start,\"end\",Color,User_Id) VALUES ('" + events.ID + "','" + events.Title + "','" + events.Description + "'," + events.AllDay + "," + events.Start + "," + events.End + ",'" + events.Color + "', '" + query2[0].Id + "')");
							context.Database.ExecuteSqlCommand(
							"INSERT INTO Event (ID,Title,Description,AllDay,\"Start\",\"end\",Color,User_Id) VALUES ('" + events.ID + "','" + events.Title + "','" + events.Description + "','" + events.AllDay + "','" + events.Start + "','" + events.End + "','" + events.Color + "', '" + query2[0].Id+"')"
							);
						}
						else if(mode == "update")
						{
							Debug.WriteLine("UPDATE Event SET ID = '" + events.ID + "', Title = '" + events.Title + "', Description = '" + events.Description + "', AllDay = '" + events.AllDay + "', Start = '" + events.Start + "', End = '" + events.End + "', Color = '" + events.Color + "', User_ID = '" + query2[0].Id + "' WHERE ID = '" + id + "'");
							context.Database.ExecuteSqlCommand(
								"UPDATE Event SET ID = '" + events.ID + "', Title = '" + events.Title + "', Description = '" + events.Description + "', AllDay = '" + events.AllDay + "', \"Start\" = '" + events.Start + "', \"End \"= '" + events.End + "', Color = '" + events.Color + "', User_ID = '" + query2[0].Id +"' WHERE ID = '"+ id + "'"
							);
						}
						else if(mode == "delete")
						{
							context.Database.ExecuteSqlCommand(
								"DELETE FROM Event WHERE ID = '"+ events.ID + "'"
							);
						}
						

						context.SaveChanges();
						dbContextTransaction.Commit();
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
						dbContextTransaction.Rollback();
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