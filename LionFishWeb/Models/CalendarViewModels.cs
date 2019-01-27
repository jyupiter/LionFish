using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class CalendarViewModel
    {

		public string ID { get; set; }
		public DateTime DateT { get; set; }
		public Dictionary<string, string> Notes { get; set; }
	}

    public class SetEventViewModel
    {
		
        public string ID { get; set; }
    }
}