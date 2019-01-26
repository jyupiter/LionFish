﻿using LionFishWeb.Models;
using LionFishWeb.Utility;
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
	[Authorize]
	public class CalendarController : Controller
	{
		private static string EventID;
		private static string SearchString;

		public ActionResult Calendar()
		{
			List<Event> listOfEvents = new List<Event>();
			listOfEvents = LoadPrivate(User.Identity.GetUserId());
			foreach (Event events in listOfEvents)
			{

				Dictionary<string, string> notesList = new Dictionary<string, string>();
				List<Note> note = new List<Note>();
				note = GetNotes(events.ID);
				events.ID = events.ID.GetIndirectReference();
				try
				{
					foreach (Note notes in note)
					{
						notesList.Add(notes.ID, notes.Title);
						Debug.WriteLine(notes.ID);
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
					Debug.WriteLine("No notes found!");
				}
				events.Notes = JsonConvert.SerializeObject(notesList);
			}
			ViewData["Events"] = listOfEvents;

			listOfEvents = LoadPublic(User.Identity.GetUserId());
			foreach (Event events in listOfEvents)
			{
				Debug.WriteLine(events.ID);
				events.ID = events.ID.GetIndirectReference();
				Debug.WriteLine(events.ID);
			}

			ViewData["EventsPublic"] = listOfEvents;

			CalendarViewModel CVM = new CalendarViewModel
			{
				ID = EventID,
				DateT = GetEventDate(EventID).Start
			};
			return View("Calendar", CVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Search([Bind(Include = "Search")] string search)
		{
			SearchString = search;
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Apply([Bind(Include = "ID, Title, Color, Description, AllDay, Start, End")] Event events)
		{
			Save(events, "update", User.Identity.GetUserId());
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "Title, Color, Description, AllDay, Start, End")] Event events)
		{
			Save(events, "create", User.Identity.GetUserId());
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete([Bind(Include = "ID")] Event events)
		{
			Save(events, "delete", User.Identity.GetUserId());
			return RedirectToAction("Calendar");
		}

		public ActionResult Note([Bind(Include = "ID, Notes")] Event events)
		{

			CallDB("UPDATE Event SET Notes = '" + events.Notes + "' WHERE ID = '" + events.ID.GetDirectReference() + "'");
			return RedirectToAction("Calendar");
		}

		public ActionResult Publish([Bind(Include = "Title, Color, Description, AllDay, Start, End")] Event events)
		{
			Save(events, "publish", User.Identity.GetUserId());
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[AllowAnonymous]
		public void SetEventByID(SetEventViewModel model)
		{
			EventID = model.ID;
		}

		[HttpPost]
		[AllowAnonymous]
		public void AutoSave(Event stuff)
		{
			string json = new JavaScriptSerializer().Serialize(stuff);
			Event events = JsonConvert.DeserializeObject<Event>(json);
			Save(events, "update", User.Identity.GetUserId());

		}


		public static List<Note> GetNotes(string EID)
		{
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						var query = context.Notes.SqlQuery("SELECT * FROM Note WHERE EventID = '" + EID + "'").ToList<Note>();
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

		public static List<Event> LoadPrivate(string user)
		{
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						var query = context.Events.SqlQuery("SELECT * FROM Event WHERE UserID = '" + user + "'").ToList<Event>();
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

		public static List<Event> LoadPublic(string user)
		{
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						List<Event> query = new List<Event>();
						List<Event> listOfEvents = new List<Event>();
						Debug.WriteLine(SearchString);
						if (SearchString != null)
						{
							query = context.Events.SqlQuery("SELECT * FROM Event WHERE Title LIKE '%" + SearchString + "%'").ToList<Event>();

						}
						else
						{
							query = context.Events.SqlQuery("SELECT * FROM Event").ToList<Event>();
							
						}
						foreach (Event events in query)
						{

							if (events.Public)
							{
								Debug.WriteLine(events.Public);
								listOfEvents.Add(events);
							}
						}
						return listOfEvents;
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
					}
				}
			}
			return null;
		}

		public static Event GetEventDate(string id)
		{
			using (var context = new ApplicationDbContext())
			{
				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						var query = context.Events.SqlQuery("SELECT * FROM Event WHERE \"Public\" = '" + id + "'").ToList<Event>();
						return query[0];
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
					}
				}
			}
			return null;
		}


		public static void DebugEvents(Event events)
		{
			string start = events.Start.ToString("MM/dd/yyyy hh:mm tt");
			string end = events.End.ToString("MM/dd/yyyy hh:mm tt");
			Debug.WriteLine("\n --------------------- \n");
			Debug.WriteLine("id: " + events.ID);
			Debug.WriteLine("title: " + events.Title);
			Debug.WriteLine("desc: " + events.Description);
			Debug.WriteLine("start: " + start);
			Debug.WriteLine("End: " + end);
			Debug.WriteLine("Notes: " + events.Notes);
			Debug.WriteLine("\n --------------------- \n");
		}

		public static bool Save(Event events, string mode, string user)
		{
			string start = events.Start.ToString("MM/dd/yyyy hh:mm tt");
			string end = events.End.ToString("MM/dd/yyyy hh:mm tt");
			DebugEvents(events);

			if (mode == "create")
			{

				CallDB("INSERT INTO Event (ID,Title,Description,AllDay,\"Start\",\"end\",Color,UserID,\"Public\") VALUES ('" + events.ID + user + "','" + events.Title + "','" + events.Description + "','" + events.AllDay + "','" + start + "','" + end + "','" + events.Color + "', '" + user + "' , 'False')");
			}
			else if (mode == "update")
			{
				CallDB("UPDATE Event SET ID = '" + events.ID.GetDirectReference() + "', Title = '" + events.Title + "', Description = '" + events.Description + "', AllDay = '" + events.AllDay + "', \"Start\" = '" + start + "', \"End \"= '" + end + "', Color = '" + events.Color + "', UserID = '" + user + "' WHERE ID = '" + events.ID.GetDirectReference() + "'");
			}
			else if (mode == "delete")
			{

				CallDB("DELETE FROM Event WHERE ID = '" + events.ID.GetDirectReference() + "'");
			}
			else if (mode == "publish")
			{
				CallDB("INSERT INTO Event (ID,Title,Description,AllDay,\"Start\",\"end\",Color,UserID,\"Public\") VALUES ('" + events.ID + user + "','" + events.Title + "','" + events.Description + "','" + events.AllDay + "','" + start + "','" + end + "','" + events.Color + "', 'public' , 'True')");

			}
			else
			{
				return false;
			}
			return true;

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