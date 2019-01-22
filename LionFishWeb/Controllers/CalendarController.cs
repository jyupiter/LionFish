using LionFishWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace LionFishWeb.Controllers
{
	public class CalendarController : Controller
	{

		// GET: Calendar
		public const string path = "D:\\Github\\LionFishWeb\\Events";

		public ActionResult Calendar()
		{
			List<Event> listOfEvents = new List<Event>();
			listOfEvents = Load();
			ViewData["Events"] = Load();
			//Debug.WriteLine("calendar() called");
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
			Save(events, "update");
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "Title, Color, Description, AllDay, Start, End")] Event events)
		{
			Debug.WriteLine(events.Title);
			Save(events, "create");
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete([Bind(Include = "ID")] Event events)
		{
			Debug.WriteLine(events.ID);
			Save(events, "delete");
			return RedirectToAction("Calendar");
		}

		public ActionResult Note([Bind(Include = "ID, Notes")] Event events)
		{
			Debug.WriteLine(events.ID);
			Debug.WriteLine(events.Notes);
			string user = SetUser();
			string start = events.Start.ToString("MM/dd/yyyy hh:mm tt");
			string end = events.End.ToString("MM/dd/yyyy hh:mm tt");
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						context.Database.ExecuteSqlCommand(
								"UPDATE Event SET Notes = '" + events.Notes + "' WHERE ID = '" + events.ID + "'"
							);
						context.SaveChanges();
						dbContextTransaction.Commit();
					}
					catch (Exception e)
					{
						dbContextTransaction.Rollback();
					}
				}
			}


			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[AllowAnonymous]
		public void AutoSave(Event stuff)
		{
			string json = new JavaScriptSerializer().Serialize(stuff);
			Debug.WriteLine("savng... " + json);
			Event events = JsonConvert.DeserializeObject<Event>(json);
			Save(events, "update");

		}

		public static List<Event> Load()
		{
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						var query = context.Events.SqlQuery("SELECT ID,Title,Description,AllDay,\"Start\",\"end\",Color,Notes FROM Event").ToList<Event>();
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
		public static string SetUser()
		{
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
						return query2[0].Id;
					}
					catch (Exception e)
					{
						return "";
					}
				}
			}
		}
		public static void Save(Event events, string mode)
		{

			Debug.WriteLine(events.Start);
			string start = events.Start.ToString("MM/dd/yyyy hh:mm tt");
			string end = events.End.ToString("MM/dd/yyyy hh:mm tt");
			string user = SetUser();

			if (mode == "create")
			{

				CallDB("INSERT INTO Event (ID,Title,Description,AllDay,\"Start\",\"end\",Color,User_Id) VALUES ('" + events.ID + "','" + events.Title + "','" + events.Description + "','" + events.AllDay + "','" + start + "','" + end + "','" + events.Color + "', '" + user + "')");
			}
			else if (mode == "update")
			{
				Debug.WriteLine("UPDATE Event SET ID = '" + events.ID + "', Title = '" + events.Title + "', Description = '" + events.Description + "', AllDay = '" + events.AllDay + "', Start = '" + start + "', End = '" + end + "', Color = '" + events.Color + "', User_ID = '" + user + "' WHERE ID = '" + events.ID + "'");

				CallDB("UPDATE Event SET ID = '" + events.ID + "', Title = '" + events.Title + "', Description = '" + events.Description + "', AllDay = '" + events.AllDay + "', \"Start\" = '" + start + "', \"End \"= '" + end + "', Color = '" + events.Color + "', User_ID = '" + user + "' WHERE ID = '" + events.ID + "'");
			}
			else if (mode == "delete")
			{

				CallDB("DELETE FROM Event WHERE ID = '" + events.ID + "'");
			}


		}

		public static void CallDB(string command)
		{
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						Debug.WriteLine(command);
						context.Database.ExecuteSqlCommand(command);
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
		#endregion
	}
}