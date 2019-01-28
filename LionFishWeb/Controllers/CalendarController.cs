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
			Logging.Log("---------- Calendar() called, Starting calendar app! ----------");
			List<Event> listOfEvents = new List<Event>();
			listOfEvents = LoadPrivate(User.Identity.GetUserId());
			Dictionary<string, string> notesList = new Dictionary<string, string>();
			List<Note> note = new List<Note>();
			Logging.Log("~~~~~~~~~~ Loading private events ~~~~~~~~~~");


			foreach (Event events in listOfEvents)
			{
				DebugEvents(events);
				
				note = GetNotes(events.ID);
				events.ID = events.ID.GetIndirectReference();
				try
				{
					foreach (Note notes in note)
					{
						Debug.WriteLine(notes.ID.GetIndirectReference());
						Debug.WriteLine("notes.EventID : " + notes.EventID.GetIndirectReference());
						Debug.WriteLine("events.ID : " + events.ID);
						Dictionary<string, string> ary = new Dictionary<string, string>();
						ary.Add(notes.ID.GetIndirectReference(), notes.EventID.GetIndirectReference());
						notesList.Add(JsonConvert.SerializeObject(ary), notes.Title);
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}
				events.Notes = JsonConvert.SerializeObject(notesList);
			}
			ViewData["Events"] = listOfEvents;
			notesList = new Dictionary<string, string>();
			Logging.Log("~~~~~~~~~~ Loading public events ~~~~~~~~~~");
			listOfEvents = LoadPublic(User.Identity.GetUserId());
			foreach (Event events in listOfEvents)
			{
				DebugEvents(events);
				events.ID = events.ID.GetIndirectReference();
				//events.UserID = User.Identity.GetUserId();
			}

			ViewData["EventsPublic"] = listOfEvents;
			DateTime tempDT = DateTime.Today;
			if (EventID != null)
			{
				tempDT = GetEventDate(EventID).Start;
			}

			note = GetNotes("");
			try
			{
				foreach (Note notes in note)
				{
					Debug.WriteLine(notes.ID);
					notesList.Add(notes.ID.GetIndirectReference(), notes.Title);
					
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}

			CalendarViewModel CVM = new CalendarViewModel
			{
				ID = EventID,
				DateT = tempDT,
				Notes = notesList,
                ProfileImg = Utility.Constants.GetProfileImg(User.Identity.GetUserId())
			};
			return View("Calendar", CVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Search([Bind(Include = "Search")] string search)
		{
			Logging.Log("User search: " + search);
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
            Debug.WriteLine("events.ID : " + events.ID);
			Save(events, "delete", User.Identity.GetUserId());
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Note([Bind(Include = "ID, Notes")] Event events)
		{
			SqlParameter ENotes = new SqlParameter
			{
				ParameterName = "@Notes",
				Value = events.Notes.GetDirectReference()
			};
			Debug.WriteLine("Notes : " + events.Notes.GetDirectReference());
			Debug.WriteLine("ID : " + events.ID.GetDirectReference());
			SqlParameter DEID = new SqlParameter
			{
				ParameterName = "@DID",
				Value = events.ID.GetDirectReference()
			};

			SqlCommand cmd = new SqlCommand("UPDATE Note SET EventID = @DID WHERE ID = @Notes");
			cmd.Parameters.Add(ENotes);
			cmd.Parameters.Add(DEID);
			CallDB(cmd);
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Publish([Bind(Include = "Title, Color, Description, AllDay, Start, End")] Event events)
		{
			Save(events, "publish", User.Identity.GetUserId());
			return RedirectToAction("Calendar");
		}

		[HttpPost]
		[AllowAnonymous]
		public void SetEventByID(SetEventViewModel model)
		{
			EventID = model.ID.GetDirectReference();
		}

		[HttpPost]
		[AllowAnonymous]
		public void AutoSave(Event stuff)
		{
			string json = new JavaScriptSerializer().Serialize(stuff);
			Event events = JsonConvert.DeserializeObject<Event>(json);
			Debug.WriteLine(events.Start);
			Debug.WriteLine(events.End);
			if (events.End ==Convert.ToDateTime("1/1/ 0001 12:00:00 AM")) { 
				events.End = events.Start;
			}
			Save(events, "update", User.Identity.GetUserId());

		}


		public static List<Note> GetNotes(string EID)
		{
			Logging.Log("~~~~~~~~~~ Getting Notes ~~~~~~~~~~");
			List<Note> listOfNotes = new List<Note>();
			
			using (SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
			{
				SqlCommand command = new SqlCommand();

				if (EID != "")
				{
					SqlParameter ID = new SqlParameter
					{
						ParameterName = "@id",
						Value = EID
					};
					command = new SqlCommand("SELECT * FROM Note WHERE EventID =  @id", conn);

					command.Parameters.Add(ID);
				}
				else
				{
					command = new SqlCommand("SELECT * FROM Note", conn);
				}
				
				conn.Open();
				SqlDataReader results = command.ExecuteReader();

				while (results.Read())
				{
					Note note = new Note();
					note.ID = results["ID"].ToString();
					note.EventID = results["EventID"].ToString();
					note.Title = results["Title"].ToString();
					Logging.Log(note.Title);
					Logging.Log(note.ID);
					listOfNotes.Add(note);
				}
				conn.Close();
			}
			return listOfNotes;
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
				Logging.Log(">>>>> SQL command: " + command.CommandText);
				command.Parameters.Add(UID);
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
				Logging.Log(">>>>> SQL command: " + command.CommandText);
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
				Logging.Log(">>>>> SQL command: " + command.CommandText);
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

			Logging.Log("\n --------------------- \n");
			Logging.Log("id: " + events.ID);
			Logging.Log("User: " + events.UserID);
			Logging.Log("title: " + events.Title);
			Logging.Log("desc: " + events.Description);
			Logging.Log("start: " + start);
			Logging.Log("End: " + end);
			Logging.Log("Notes: " + events.Notes);
			Logging.Log("\n --------------------- \n");
		}

		public static bool Save(Event events, string mode, string user2)
		{
			string start = events.Start.ToString("MM/dd/yyyy hh:mm tt");
			string end = events.End.ToString("MM/dd/yyyy hh:mm tt");
			Debug.WriteLine(end);
			Debug.WriteLine(events.End.ToString("MM/dd/yyyy hh:mm tt"));
			Debug.WriteLine("end == events.End.ToString(\"MM / dd / yyyy hh: mm tt\") : " + events.End.ToString("MM/dd/yyyy hh:mm tt") == "01 / 01 / 0001 12:00 AM");
			if (events.End.ToString("MM/dd/yyyy hh:mm tt") == "01 / 01 / 0001 12:00 AM")
			{
				end = start;
			}
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
				Logging.Log("Creating event!");
				CallDB(cmd);
			}
			else if (mode == "update")
			{
				Debug.WriteLine(events.Title);
				Debug.WriteLine(start);
				Debug.WriteLine(end);
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
				Logging.Log("Updating event!");
				CallDB(cmd);
			}
			else if (mode == "delete")
			{
                Debug.WriteLine(events.ID);
				SqlParameter DEID = new SqlParameter
				{
					ParameterName = "@DID",
					Value = events.ID.GetDirectReference()
				};
				SqlCommand cmd = new SqlCommand(
				"DELETE FROM Event WHERE ID =  @DID ");
				cmd.Parameters.Add(DEID);
				Logging.Log("Deleting event!");
				CallDB(cmd);
			}
			else if (mode == "publish")
			{
				SqlCommand cmd = new SqlCommand(
				"INSERT INTO Event (ID,Title,Description,AllDay,\"start\",\"end\",Color,UserID,\"Public\") VALUES ( @ID , @Title , @Description , @AllDay , @start , @end , @Color , 'public' , 'True')");
				cmd.Parameters.Add(EID);
				cmd.Parameters.Add(ETitle);
				cmd.Parameters.Add(EDesc);
				cmd.Parameters.Add(EAllDay);
				cmd.Parameters.Add(EStart);
				cmd.Parameters.Add(EEnd);
				cmd.Parameters.Add(EColor);
				cmd.Parameters.Add(EUserID);
				Logging.Log("Publishing event!");
				CallDB(cmd);
			}
			else
				return false;
			return true;
		}

		public static void CallDB(SqlCommand command)
		{
			try { 
			using (SqlConnection conn = new SqlConnection(Utility.Constants.Conn))
			{
				Logging.Log(">>>>> SQL command: " + command.CommandText);
				conn.Open();
				command.Connection = conn;
				command.ExecuteNonQuery();
				conn.Close();
			}
			}
			catch(Exception e)
			{
				Debug.WriteLine(e);
			}
		}

		#region Helpers
		#endregion
	}
}