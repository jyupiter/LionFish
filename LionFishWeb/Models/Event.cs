using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace LionFishWeb.Models
{
    public class Event
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public string ID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool AllDay { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public string Color { get; set; }
		public Dictionary<string, string> Notes { get; set; }
		public string UserID { get; set; }
		public bool Public { get; set; }
		public Event()
		{
			UserID = "";
			ID = UserID + Guid.NewGuid();
			Public = false;
			Notes = new List<string>();
		}
		public Event(string title, string color, string desc)
		{
			Title = title;
			Color = color;
			Description = desc;
		}

		public Event(string id, string title, string desc, bool allDay, DateTime start, DateTime end, string color)
		{

		}

		public static string GetJson(Event events)
		{
			return JsonConvert.SerializeObject(events);
		}

		public static Event UnJson(string json)
		{
			return JsonConvert.DeserializeObject<Event>(json);
		}

		public static void WriteToFile(string thing, string name)
		{
			System.IO.File.WriteAllText(@"D:\Github\LionFish\Events\" + name + ".json", thing + "\n");
		}
	}
}