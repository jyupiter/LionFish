using LionFishWeb.Models;
using LionFishWeb.Utility;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Configuration;


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
			DateTime tempDT = DateTime.Today;
			if(EventID != null)
			{
				tempDT = GetEventDate(EventID).Start;
			}
			CalendarViewModel CVM = new CalendarViewModel
			{
				ID = EventID,
				DateT = tempDT
			};
			Debug.WriteLine(EventID);
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
			Debug.WriteLine(User.Identity.GetUserId());
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
			SqlParameter ENotes = new SqlParameter
			{
				ParameterName = "@Notes",
				Value = events.Notes
			};

			SqlParameter DEID = new SqlParameter
			{
				ParameterName = "@DID",
				Value = events.Notes
			};

			SqlCommand cmd = new SqlCommand("UPDATE Event SET Notes = @Notes WHERE ID = @DID");
			cmd.Parameters.Add(ENotes);
			cmd.Parameters.Add(DEID);
			CallDB(cmd);
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
			List<Event> listOfEvents = new List<Event>();
			SqlParameter UID = new SqlParameter
			{
				ParameterName = "@user",
				Value = user
			};
			using (SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
			{
				SqlCommand command = new SqlCommand();

				command = new SqlCommand("SELECT * FROM Event WHERE UserID = @user", conn);

				command.Parameters.Add(UID);
				Debug.WriteLine(command.CommandText);
				conn.Open();
				SqlDataReader results = command.ExecuteReader();
				
				while (results.Read())
				{
					Event events = new Event();
					events.ID = results["ID"].ToString();
					events.Title = results["Title"].ToString();
					events.Description = results["Description"].ToString();
					events.AllDay = Convert.ToBoolean(results["AllDay"].ToString());
					events.Start = Convert.ToDateTime(results["Start"].ToString());
					events.End = Convert.ToDateTime(results["End"].ToString());
					events.Color = results["Color"].ToString();
					events.UserID = results["UserID"].ToString();
					events.Public = Convert.ToBoolean(results["Public"].ToString());
					listOfEvents.Add(events);
				}
				conn.Close();
			}
			return listOfEvents;
		}

		public static List<Event> LoadPublic(string user)
		{
			List<Event> listOfEvents = new List<Event>();
			SqlParameter search = new SqlParameter
			{
				ParameterName = "@search",
				Value = SearchString
			};
			using (SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
			{
				SqlCommand command = new SqlCommand();
				if (SearchString != null)
				{
					command = new SqlCommand("SELECT * FROM Event WHERE Title LIKE %@search%", conn);
					command.Parameters.Add(search);
				}
				else
				{
					command = new SqlCommand("SELECT * FROM Event", conn);
				}
				
				Debug.WriteLine(command.CommandText);
				conn.Open();
				SqlDataReader results = command.ExecuteReader();
				
				while (results.Read())
				{
					if (results["Public"].ToString() == "True")
					{
						Event events = new Event();
						events.ID = results["ID"].ToString();
						events.Title = results["Title"].ToString();
						events.Description = results["Description"].ToString();
						events.AllDay = Convert.ToBoolean(results["AllDay"].ToString());
						events.Start = Convert.ToDateTime(results["Start"].ToString());
						events.End = Convert.ToDateTime(results["End"].ToString());
						events.Color = results["Color"].ToString();
						events.UserID = results["UserID"].ToString();
						events.Public = Convert.ToBoolean(results["Public"].ToString());
						listOfEvents.Add(events);
					}
				}
				conn.Close();
			}

			return listOfEvents;
		}

		public static Event GetEventDate(string id)
		{
			if (id == null) { return null; }
			SqlParameter uid = new SqlParameter
			{
				ParameterName = "@UID",
				Value = id
			};
			using (SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
			{
				SqlCommand command = new SqlCommand();
				command = new SqlCommand("SELECT * FROM Event WHERE ID = @UID", conn);
				command.Parameters.Add(uid);

				Debug.WriteLine(command.CommandText);
				conn.Open();
				SqlDataReader results = command.ExecuteReader();
				Event events = new Event();
				while (results.Read())
				{

					events.ID = results["ID"].ToString();
					events.Title = results["Title"].ToString();
					events.Description = results["Description"].ToString();
					events.AllDay = Convert.ToBoolean(results["AllDay"].ToString());
					events.Start = Convert.ToDateTime(results["Start"].ToString());
					events.End = Convert.ToDateTime(results["End"].ToString());
					events.Color = results["Color"].ToString();
					events.UserID = results["UserID"].ToString();
					events.Public = Convert.ToBoolean(results["Public"].ToString());
				}
				conn.Close();

				return events;
			}
		}

		public static void DebugEvents(Event events)
		{
			string start = events.Start.ToString("MM/dd/yyyy hh:mm tt");
			string end = events.End.ToString("MM/dd/yyyy hh:mm tt");
			Debug.WriteLine("\n --------------------- \n");
			Debug.WriteLine("id: " + events.ID);
			Debug.WriteLine("User: " + events.UserID);
			Debug.WriteLine("title: " + events.Title);
			Debug.WriteLine("desc: " + events.Description);
			Debug.WriteLine("start: " + start);
			Debug.WriteLine("End: " + end);
			Debug.WriteLine("Notes: " + events.Notes);
			Debug.WriteLine("\n --------------------- \n");
		}

		public static bool Save(Event events, string mode, string user2)
		{
			string start = events.Start.ToString("MM/dd/yyyy hh:mm tt");
			string end = events.End.ToString("MM/dd/yyyy hh:mm tt");
			DebugEvents(events);
			Debug.WriteLine(user2);
			SqlParameter user = new SqlParameter
			{
				ParameterName = "@User",
				Value = user2
			};

			SqlParameter EID = new SqlParameter
			{
				ParameterName = "@ID",
				Value = events.ID
			};
			SqlParameter ETitle = new SqlParameter
			{
				ParameterName = "@Title",
				Value = events.Title
			};
			SqlParameter EDesc = new SqlParameter
			{
				ParameterName = "@Description",
				Value = events.Description
			};
			SqlParameter EAllDay = new SqlParameter
			{
				ParameterName = "@AllDay",
				Value = events.AllDay
			};
			SqlParameter EStart = new SqlParameter
			{
				ParameterName = "@Start",
				Value = start
			};
			SqlParameter EEnd = new SqlParameter
			{
				ParameterName = "@End",
				Value = end
			};
			SqlParameter EColor = new SqlParameter
			{
				ParameterName = "@Color",
				Value = events.Color
			};
			SqlParameter EUserID = new SqlParameter
			{
				ParameterName = "@UserID",
				Value = events.UserID
			};
			SqlParameter EPublic = new SqlParameter
			{
				ParameterName = "@Public",
				Value = events.Public
			};

			if (mode == "create")
			{
				SqlCommand cmd = new SqlCommand(
				"INSERT INTO Event (ID,Title,Description,AllDay,\"start\",\"end\",Color,UserID,\"Public\") VALUES ( @ID ,@Title ,@Description,@AllDay,@start ,@end , @Color , @User , 'False')"
				);
				cmd.Parameters.Add(EID);
				cmd.Parameters.Add(ETitle);
				cmd.Parameters.Add(EDesc);
				cmd.Parameters.Add(EAllDay);
				cmd.Parameters.Add(EStart);
				cmd.Parameters.Add(EEnd);
				cmd.Parameters.Add(EColor);
				cmd.Parameters.Add(user);
				CallDB(cmd);
			}
			else if (mode == "update")
			{
				DebugEvents(events);
				SqlParameter DEID = new SqlParameter
				{
					ParameterName = "@DID",
					Value = events.ID.GetDirectReference()
				};
				SqlCommand cmd = new SqlCommand(
				"UPDATE Event SET Title =  @Title , Description =  @Description , AllDay =  @AllDay , \"start\" =  @start , \"end \"=  @end , Color =  @Color , UserID =  @User  WHERE ID =  @DID ");
				cmd.Parameters.Add(EID);
				cmd.Parameters.Add(ETitle);
				cmd.Parameters.Add(EDesc);
				cmd.Parameters.Add(EAllDay);
				cmd.Parameters.Add(EStart);
				cmd.Parameters.Add(EEnd);
				cmd.Parameters.Add(EColor);
				cmd.Parameters.Add(user);
				cmd.Parameters.Add(DEID);
				CallDB(cmd);
			}
			else if (mode == "delete")
			{
				SqlParameter DEID = new SqlParameter
				{
					ParameterName = "@DID",
					Value = events.ID.GetDirectReference()
				};
				SqlCommand cmd = new SqlCommand(
				"DELETE FROM Event WHERE ID =  @DID ");
				cmd.Parameters.Add(DEID);

				CallDB(cmd);
			}
			else if (mode == "publish")
			{
				SqlCommand cmd = new SqlCommand(
				"INSERT INTO Event (ID,Title,Description,AllDay,\"start\",\"end\",Color,UserID,\"Public\") VALUES ( @ID , @Title , @Description , @AllDay , @start , @end , @Color , 'public' , True)");
				cmd.Parameters.Add(EID);
				cmd.Parameters.Add(ETitle);
				cmd.Parameters.Add(EDesc);
				cmd.Parameters.Add(EAllDay);
				cmd.Parameters.Add(EStart);
				cmd.Parameters.Add(EEnd);
				cmd.Parameters.Add(EColor);
				cmd.Parameters.Add(EUserID);
				CallDB(cmd);
			}
			else
                return false;
			return true;
		}

		public static void CallDB(SqlCommand command)
		{
			using (SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
			{
				Debug.WriteLine(command.CommandText);
				conn.Open();
				command.Connection = conn;
				command.ExecuteNonQuery();
				conn.Close();
			}

		}

		#region Helpers
		#endregion
	}
}