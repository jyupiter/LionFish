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
		public string Notes { get; set; }
		public string UserID { get; set; }
		public bool Public { get; set; }
		public Event()
		{
			UserID = "";
			ID = UserID + Guid.NewGuid();
			Public = false;
		}

		public static string GetJson(Event events)
		{
			return JsonConvert.SerializeObject(events);
		}

		public static Event UnJson(string json)
		{
			return JsonConvert.DeserializeObject<Event>(json);
		}
	}
}